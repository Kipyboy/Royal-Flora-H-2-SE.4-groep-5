"use client";
import React, { useEffect, useState } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";
import Topbar from "../components/Topbar";
import Clock from "../components/clock";
import '../../styles/Veiling.css';
import VeilingSidebar from "../components/veiling-sidebar";
import { getUser } from "../utils/auth";
import { API_BASE_URL } from "../config/api";

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

  const [elapsed, setElapsed] = useState(0);
  const [startTime, setStartTime] = useState<number | null>(null);
  const [currentPrice, setCurrentPrice] = useState<number | null>(null);

  useEffect(() => { const user = getUser(); if (!user) window.location.href = "/login"; }, []);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/KlokkenHub`)
      .withAutomaticReconnect()
      .build();

    connection.on("ClockUpdate", (data) => {
      if (data.id === config.clockId) {
        setElapsed(data.elapsed);
        setStartTime(Date.parse(data.startTime));
      }
    });

    (async () => { try { await connection.start(); } catch (err) { console.error("SignalR connection error:", err); } })();

    return () => {
      connection.stop().catch(err => console.error("SignalR stop error:", err));
    };
  }, [config.clockId]);

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
        <Clock
          elapsed={elapsed}
          startTime={startTime}
          durationMs={10000}
          onPriceChange={setCurrentPrice}
          locationName={config.locationName}
          onFinished={handleClockFinished}
        />
      </div>

      <VeilingSidebar
        locationName={config.locationName}
        verkoopPrijs={currentPrice}
      />
    </div>
  );
}