'use client';

import React, { useEffect, useState } from 'react';
import Head from 'next/head';
import '../../styles/homepage.css';
import Topbar from '../components/Topbar';
import ProductCard from '../components/product-card';
import GekochtProductCard from '../components/gekocht-product-card';
import EigenProductCard from '../components/eigen-product-card';
import VeilingmeesterProductCard from '../components/veilingmeester-product-card';
import { authFetch } from '../utils/api';
import { getUser } from '../utils/auth';
import { API_BASE_URL } from '../config/api';

interface User {
  username: string;
  email: string;
  role: string;
  KVK?: string;
}

// Startpagina: toont lijsten met producten en biedt filtermogelijkheden.
// Filters worden als 'controlled' props naar de sidebars doorgegeven.
// `reloadProducts` haalt de productlijst op van de backend en gebruikt `authFetch`
// zodat, indien aanwezig, de JWT-autorization header wordt meegestuurd.
const HomePage: React.FC = () => {
  // Component state:
  // - products: ontvangen productlijst van de API
  // - user: huidige ingelogde gebruiker (minimaal info uit localStorage)
  // - loading: toont laadindicator bij asynchrone bewerkingen
  const [products, setProducts] = useState<any[]>([]);
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Filters (controlled state die door de sidebars wordt aangepast):
  // - tonen / niet tonen (aankomend, eigen, gekocht, in te plannen)
  // - locaties (A/B/C/D)
  // - tekstfilters voor datum, merk en naam
  const [aankomendChecked, setAankomendChecked] = useState(true);
  const [eigenChecked, setEigenChecked] = useState(false);
  const [gekochtChecked, setGekochtChecked] = useState(false);
  const [inTePlannenChecked, setInTePlannenChecked] = useState(false);
  const [aChecked, setAChecked] = useState(false);
  const [bChecked, setBChecked] = useState(false);
  const [cChecked, setCChecked] = useState(false);
  const [dChecked, setDChecked] = useState(false);
  const [dateFilter, setDateFilter] = useState('');
  const [merkFilter, setMerkFilter] = useState('');
  const [naamFilter, setNaamFilter] = useState('');
  const [sidebarVisible, setSidebarVisible] = useState(true);

  const [toonBeschrijving, setToonBeschrijving] = useState(false);
  const [auctionsInactive, setAuctionsInactive] = useState(true);

  // Haalt producten op van de backend en zet de lokale state. `authFetch` voegt
  // automatisch de Authorization header toe wanneer er een token aanwezig is.
  // Errors en JSON parse fouten worden gelogd naar de console.
  const reloadProducts = async () => {
    setLoading(true);
    const storedUser = getUser();
    if (!storedUser) {
      setLoading(false);
      setUser(null);
      return;
    }
    setUser(storedUser);

    try {
      const response = await authFetch(`${API_BASE_URL}/api/Products`);
      if (!response || !response.ok) {
        const text = await response.text();
        console.error('Failed to fetch products', response?.status, text);
      } else {
        try {
          const data = await response.json();
          setProducts(Array.isArray(data) ? data : []);
        } catch (parseErr) {
          const text = await response.text();
          console.error('Failed to parse products JSON:', parseErr, 'Response text:', text);
        }
      }
    } catch (err) {
      console.error('Error fetching products', err);
    } finally {
      setLoading(false);
    }
  }; 

  // Bij mount: laad producten en controleer of veilingen gepauzeerd zijn zodat de
  // start/pauze-knop juiste initiële state heeft.
  useEffect(() => {
    reloadProducts();

    // Check if any auctions are paused to set initial button state
    const checkPaused = async () => {
      try {
        const resp = await authFetch(`${API_BASE_URL}/api/Products/HasPausedAuctions`);
        if (resp && resp.ok) {
          const json = await resp.json();
          setAuctionsInactive(!json);
        }
      } catch (e) {
        console.error('Failed to check paused auctions', e);
      }
    };

    checkPaused();
  }, []);

  
  // Centraliseer checkbox events van de sidebars. We gebruiken de `id` van de
  // checkbox om te bepalen welke filter geüpdatet moet worden. Sidebars zijn stateless.
  const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, checked } = e.target;
    if (id === 'Aankomende producten') setAankomendChecked(checked);
    if (id === 'Eigen producten') setEigenChecked(checked);
    if (id === 'Gekochte producten') setGekochtChecked(checked);
    if (id === 'In te plannen producten') setInTePlannenChecked(checked);
    if (id === 'A') setAChecked(checked);
    if (id === 'B') setBChecked(checked);
    if (id === 'C') setCChecked(checked);
    if (id === 'D') setDChecked(checked);
  }; 

  
  // Update tekst- en datum-filters (controlled inputs van de sidebars)
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    if (id === 'datum-input') setDateFilter(value);
    if (id === 'merk-input') setMerkFilter(value);
    if (id === 'naam-input') setNaamFilter(value);
  };

    // Toggle-knop: wissel tussen product overzicht en volledige productbeschrijvingen
    const handleButtonClick: React.MouseEventHandler<HTMLButtonElement> = (e) => {
      setToonBeschrijving(!toonBeschrijving);
    }; 

  // Bepaalt welk kaartcomponent voor een product gebruikt moet worden:
  // - eerst: gekochte producten (GekochtProductCard)
  // - dan: eigen producten (EigenProductCard)
  // - voor veilingmeester: extra actiekaarten (VeilingmeesterProductCard)
  // - default: generieke ProductCard
  const renderCard = (product: any) => {
    const typeStr = (product.type ?? product.status ?? '').toString().toLowerCase();
    const isGekocht = typeStr === 'gekocht' || typeStr === 'verkocht';
    const isEigen = typeStr === 'eigen';

    if (isGekocht) {
      return (
        <GekochtProductCard
          key={product.id}
          naam={product.naam}
          beschrijving={product.beschrijving}
          merk={product.merk}
          verkoopPrijs={(product.verkoopPrijs ?? product.prijs)?.toString()}
          datum={product.datum}
          locatie={product.locatie}
          status={product.status}
          aantal={product.aantal}
          fotoPath={product.fotoPath}
          toonBeschrijving={toonBeschrijving}
        />
      );
    }

    if (isEigen) {
      return (
        <EigenProductCard
          key={product.id}
          naam={product.naam}
          beschrijving={product.beschrijving}
          merk={product.merk}
          verkoopPrijs={(product.verkoopPrijs ?? product.prijs)?.toString()}
          koper={product.koper ?? ''}
          datum={product.datum}
          locatie={product.locatie}
          status={product.status}
          aantal={product.aantal}
          fotoPath={product.fotoPath}
          toonBeschrijving={toonBeschrijving}
        />
      );
    }

    if(user?.role === 'Veilingmeester') {
      return (
        <VeilingmeesterProductCard
          id={product.id}
          key={product.id}
          naam={product.naam}
          beschrijving={product.beschrijving}
          merk={product.merk}
          prijs={product.prijs}
          datum={product.datum}
          locatie={product.locatie}
          status={product.status}
          aantal={product.aantal}
          fotoPath={product.fotoPath}
          toonBeschrijving={toonBeschrijving}
        />
      );
    }

    return (
      <ProductCard
        key={product.id}
        naam={product.naam}
        beschrijving={product.beschrijving}
        merk={product.merk}
        prijs={product.prijs}
        datum={product.datum}
        locatie={product.locatie}
        status={product.status}
        aantal={product.aantal}
        fotoPath={product.fotoPath}
        toonBeschrijving={toonBeschrijving}
      />
    );
  };

  // Past filters toe op de volledige productlijst en rendert vervolgens voor elk
  // gefilterd product het juiste kaartcomponent via `renderCard`.
  const productenInladen = () =>
    products
      .filter((product) => {
        // Filter op status/type
        if (
          (!aankomendChecked && product.status === 'Ingepland') ||
          (!eigenChecked && product.type === 'Eigen') ||
          (!gekochtChecked && product.status === 'Verkocht') ||
          (!inTePlannenChecked && product.status === 'Geregistreerd')
        ) {
          return false;
        }

        // Filter op locatie (A/B/C/D) wanneer een of meer locaties geselecteerd zijn
        if (
          (aChecked || bChecked || cChecked || dChecked) &&
          ((!aChecked && product.locatie === 'Naaldwijk') ||
            (!bChecked && product.locatie === 'Aalsmeer') ||
            (!cChecked && product.locatie === 'Rijnsburg') ||
            (!dChecked && product.locatie === 'Eelde'))
        ) {
          return false;
        }

        // Datum- en tekstfilters
        if (dateFilter && product.datum !== dateFilter) return false;
        const merkVal = (product.merk ?? '').toString();
        const naamVal = (product.naam ?? '').toString();
        if (merkFilter && !merkVal.toLowerCase().includes(merkFilter.toLowerCase())) return false;
        if (naamFilter && !naamVal.toLowerCase().includes(naamFilter.toLowerCase())) return false;

        return true;
      })
      .map((product) => renderCard(product));

  const toggleSidebar = () => setSidebarVisible(!sidebarVisible);

  // Start veilingdag: roept backend endpoint aan om veilingen te starten/hervatten
  const startDay = async () => {
      const response = await authFetch(`${API_BASE_URL}/api/Products/StartAuctions`, {method: 'POST'});
      if (!response || !response.ok) {
        console.error("Failed to start auctions", response?.status)
      }
      setAuctionsInactive(false);
  }
  // Pauzeer veilingen: backend endpoint pauzeert veilingen, daarna refreshen we producten
  const pauseAuctions = async () => {
    const response = await authFetch(`${API_BASE_URL}/api/Products/PauseAuctions`, { method: 'POST' });
    if (!response || !response.ok) {
      console.error('Failed to pause auctions', response?.status);
      return;
    }
    setAuctionsInactive(true);
    await reloadProducts();
  };

  

  if (loading) return <p>Loading...</p>;
  if (!user) return <p>Niet ingelogd. Log in om verder te gaan.</p>;

  return (
    <div className="homepage-page">
      <Head>
        <title>Home - Royal FloraHolland</title>
        <meta charSet="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      </Head>

      <Topbar
        useSideBar={true}
        currentPage="Home"
        sidebarVisible={sidebarVisible}
        toggleSidebar={toggleSidebar}
        aankomendChecked={aankomendChecked}
        eigenChecked={eigenChecked}
        gekochtChecked={gekochtChecked}
        inTePlannenChecked={inTePlannenChecked}
        aChecked={aChecked}
        bChecked={bChecked}
        cChecked={cChecked}
        dChecked={dChecked}
        dateFilter={dateFilter}
        merkFilter={merkFilter}
        naamFilter={naamFilter}
        toonBeschrijving={toonBeschrijving}
        onCheckboxChange={handleCheckboxChange}
        onInputChange={handleInputChange}
        onButtonClick={handleButtonClick}
        user={user}
      />

      <div className="main-layout">
        <div className="content">
          <div className="veilingen">
            {[
              { name: 'Naaldwijk', key: 'a' },
              { name: 'Aalsmeer', key: 'b' },
              { name: 'Rijnsburg', key: 'c' },
              { name: 'Eelde', key: 'd' },
            ].map(({ name, key }) => (
              <a key={name} href={`/veiling?loc=${key}`} className="card">
                <p>Locatie {name}</p>
                <img className = 'veiling' src={`${API_BASE_URL}/images/locatie-${key}.jpg`} alt="" />
              </a>
            ))}
            {user?.role === 'Veilingmeester' && (
            <>
              <button className='veiling-controls' onClick={auctionsInactive ? startDay : pauseAuctions}>
                {auctionsInactive ? 'Veilingen starten/hervatten' : 'Veilingen pauzeren'}
              </button>
            </>
            )}
          </div>

          <div className="producten">{productenInladen()}</div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;
