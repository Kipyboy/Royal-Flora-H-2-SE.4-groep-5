"use client";
import React, { useEffect, useState } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import Topbar from "../components/Topbar";
import Clock from "../components/clock";
import '../../styles/Veiling.css';
import VeilingSidebar from "../components/veiling-sidebar";
import { getUser } from "../utils/auth";
import { API_BASE_URL } from "../config/api";

// Veilingpagina voor een specifieke locatie.
// Deze pagina verbindt met de SignalR `KlokkenHub` om live klok-updates te ontvangen
// (tijd verstreken, starttijd). De Clock component rendert een visuele klok en
// berekent de actuele veilingprijs; wanneer de klok afloopt roept `handleClockFinished`
// een backend endpoint aan om de veiling vooruit te zetten.


// Configuratie per locatiecode: geeft locatie-naam en bijbehorende klok-id door.
const configs = {
  a: { locationName: "Naaldwijk", clockId: 1 },
  b: { locationName: "Aalsmeer", clockId: 2 },
  c: { locationName: "Rijnsburg", clockId: 3 },
  d: { locationName: "Eelde", clockId: 4 },
};

export default function VeilingPage({ searchParams }: { searchParams: Promise<{ loc?: string }> }) {
  const params = React.use(searchParams);
  const location = params.loc ?? "a";
  const config = configs[location as keyof typeof configs];

  // State voor klok/prijs:
  // - elapsed: verstreken tijd (ms) zoals door de hub gemeld
  // - startTime: starttijd van de huidige klok (wordt gebruikt door Clock component)
  // - currentPrice: actuele veilingprijs die door Clock via onPriceChange wordt doorgegeven
  const [elapsed, setElapsed] = useState(0);
  const [startTime, setStartTime] = useState<number | null>(null);
  const [currentPrice, setCurrentPrice] = useState<number | null>(null);

  // Controleer of de gebruiker is ingelogd; zo niet, redirect direct naar de login-pagina.
  // We doen dit client-side omdat deze pagina (en de SignalR connectie) alleen voor
  // geauthenticeerde gebruikers bedoeld is.
  useEffect(() => {
    const user = getUser();
    if (!user) window.location.href = "/login";
  }, []);

  // Maak een SignalR verbinding met de KlokkenHub en luister naar ClockUpdate events.
  // Wanneer een update voor de huidige klok-id binnenkomt, werken we de state bij.
  // We gebruiken automatic reconnect en stoppen de verbinding bij unmount om resource leaks te voorkomen.
  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/KlokkenHub`)
      .withAutomaticReconnect()
      .build();

    // Event-handler voor klokupdates van de hub
    connection.on("ClockUpdate", (data) => {
      if (data.id === config.clockId) {
        setElapsed(data.elapsed);
        setStartTime(Date.parse(data.startTime));
      }
    });

    // Start de verbinding asynchroon
    (async () => { try { await connection.start(); } catch (err) { console.error("SignalR connection error:", err); } })();

    // Cleanup: stop de verbinding bij unmount
    return () => {
      connection.stop().catch(err => console.error("SignalR stop error:", err));
    };
  }, [config.clockId]);

  // Wordt aangeroepen wanneer de Clock aangeeft dat de tijd voorbij is.
  // Roept backend endpoint aan om de veiling vooruit te zetten en herlaadt de pagina
  // zodat de UI de nieuwe veilingstatus en producten kan tonen.
  const handleClockFinished = async () => {
    await fetch(`${API_BASE_URL}/api/Products/Advance?locatie=${config.locationName}`, {
      method: "POST"
    });
    window.location.reload();
  }; 

  return (
    <div className="veiling-page">
      <Topbar currentPage="Veiling" useSideBar={false} />

      <div className="clock-container">
        {/* Clock component: visualiseert resterende tijd en berekent de actuele prijs.
            - elapsed / startTime worden door de SignalR handler bijgewerkt
            - onPriceChange zorgt ervoor dat de sidebar de actuele verkoopprijs ontvangt
            - onFinished wordt aangeroepen bij 0 en triggert server-side advance */}
        <Clock
          elapsed={elapsed}
          startTime={startTime}
          durationMs={10000}
          onPriceChange={setCurrentPrice}
          locationName={config.locationName}
          onFinished={handleClockFinished}
        />
      </div>

      {/* Sidebar toont het actieve product voor deze veilinglocatie en gebruikt `verkoopPrijs`
          om de gebruiker te laten bieden of direct te kopen (afhankelijk van rol) */}
      <VeilingSidebar
        locationName={config.locationName}
        verkoopPrijs={currentPrice}
      />
    </div>
  );
}