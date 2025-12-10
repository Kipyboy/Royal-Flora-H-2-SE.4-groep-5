'use client';

import React, { useEffect, useState } from 'react';
import Head from 'next/head';
import '../../styles/homepage.css';
import Topbar from '../components/Topbar';
import ProductCard from '../components/product-card';
import GekochtProductCard from '../components/gekocht-product-card';
import EigenProductCard from '../components/eigen-product-card';
import { authFetch } from '../utils/api';
import { getUser } from '../utils/auth';

interface User {
  username: string;
  email: string;
  role: string;
  KVK?: string;
}

const HomePage: React.FC = () => {
  const [products, setProducts] = useState<any[]>([]);
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Filters
  const [aankomendChecked, setAankomendChecked] = useState(true);
  const [eigenChecked, setEigenChecked] = useState(true);
  const [gekochtChecked, setGekochtChecked] = useState(true);
  const [inTePlannenChecked, setInTePlannenChecked] = useState(true);
  const [aChecked, setAChecked] = useState(false);
  const [bChecked, setBChecked] = useState(false);
  const [cChecked, setCChecked] = useState(false);
  const [dChecked, setDChecked] = useState(false);
  const [dateFilter, setDateFilter] = useState('');
  const [merkFilter, setMerkFilter] = useState('');
  const [naamFilter, setNaamFilter] = useState('');
  const [sidebarVisible, setSidebarVisible] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      const storedUser = getUser();
      if (!storedUser) {
        setLoading(false);
        return;
      }
      setUser(storedUser);

      try {
        const response = await authFetch('http://localhost:5156/api/Products');
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

    fetchData();
  }, []);

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

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    if (id === 'datum-input') setDateFilter(value);
    if (id === 'merk-input') setMerkFilter(value);
    if (id === 'naam-input') setNaamFilter(value);
  };

  const renderCard = (product: any) => {
    const typeStr = (product.type ?? product.status ?? '').toString().toLowerCase();
    const isGekocht = typeStr === 'gekocht' || typeStr === 'verkocht';
    const isEigen = typeStr === 'eigen';

    if (isGekocht) {
      return (
        <GekochtProductCard
          key={product.id}
          naam={product.naam}
          merk={product.merk}
          verkoopPrijs={(product.verkoopPrijs ?? product.prijs)?.toString()}
          datum={product.datum}
          locatie={product.locatie}
          status={product.status}
          aantal={product.aantal}
          fotoPath={product.fotoPath}
        />
      );
    }

    if (isEigen) {
      return (
        <EigenProductCard
          key={product.id}
          naam={product.naam}
          merk={product.merk}
          verkoopPrijs={(product.verkoopPrijs ?? product.prijs)?.toString()}
          koper={product.koper ?? ''}
          datum={product.datum}
          locatie={product.locatie}
          status={product.status}
          aantal={product.aantal}
          fotoPath={product.fotoPath}
        />
      );
    }

    return (
      <ProductCard
        key={product.id}
        naam={product.naam}
        merk={product.merk}
        prijs={product.prijs}
        datum={product.datum}
        locatie={product.locatie}
        status={product.status}
        aantal={product.aantal}
        fotoPath={product.fotoPath}
      />
    );
  };

  const productenInladen = () =>
    products
      .filter((product) => {
        if (
          (!aankomendChecked && product.status === 'Aankomend') ||
          (!eigenChecked && product.type === 'Eigen') ||
          (!gekochtChecked && (product.status === 'Verkocht' || product.status === 'Gekocht')) ||
          (!inTePlannenChecked && product.status === 'In te plannen')
        ) {
          return false;
        }

        if (
          (aChecked || bChecked || cChecked || dChecked) &&
          ((!aChecked && product.locatie === 'Naaldwijk') ||
            (!bChecked && product.locatie === 'Aalsmeer') ||
            (!cChecked && product.locatie === 'Rijnsburg') ||
            (!dChecked && product.locatie === 'Eelde'))
        ) {
          return false;
        }

        if (dateFilter && product.datum !== dateFilter) return false;
        const merkVal = (product.merk ?? '').toString();
        const naamVal = (product.naam ?? '').toString();
        if (merkFilter && !merkVal.toLowerCase().includes(merkFilter.toLowerCase())) return false;
        if (naamFilter && !naamVal.toLowerCase().includes(naamFilter.toLowerCase())) return false;

        return true;
      })
      .map((product) => renderCard(product));

  const toggleSidebar = () => setSidebarVisible(!sidebarVisible);

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
        onCheckboxChange={handleCheckboxChange}
        onInputChange={handleInputChange}
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
                <img className = 'veiling' src={`http://localhost:5156/images/locatie-${key}.jpg`} alt="" />
              </a>
            ))}
          </div>

          <div className="producten">{productenInladen()}</div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;
