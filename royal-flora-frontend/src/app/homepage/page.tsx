'use client';

import React, { useEffect, useState } from 'react';
import Head from 'next/head';
import ProductCard from '../components/product-card';
import '../../styles/homepage.css';
import Sidebar from '../components/Sidebar';
import ProductCard from '../components/product-card'
import GekochtProductCard from '../components/gekocht-product-card'
import EigenProductCard from '../components/eigen-product-card'
import  '../../styles/homepage.css';
import { mock } from 'node:test';
import Topbar from '../components/Topbar';
import { authFetch } from '../utils/api';
import { getUser } from '../utils/auth';
import { getToken } from '../utils/auth';

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

  

  const getProducts = async () => {
      try {
        const token = getToken();
        const headers: Record<string,string> = { "Content-Type": "application/json" };
            if (token) headers["Authorization"] = `Bearer ${token}`;
        const response = await fetch("http://localhost:5156/api/products", {
          method: "GET",
          headers
        });
        const data = await response.json();
        setProducts(data);
      }
      catch (error) {
        console.log("Fout bij producten ophalen")
      }
  };
  useEffect(() => {
    getProducts();
  }, []);

  const [aankomendChecked, setAankomendChecked] = useState(true);
  const [eigenChecked, setEigenChecked] = useState(false);
  const [gekochtChecked, setGekochtChecked] = useState(false);
  const [inTePlannenChecked, setInTePlannenChecked] = useState(false);
  const [aChecked, setAChecked] = useState(false);
  const [bChecked, setBChecked] = useState(false);
  const [cChecked, setCChecked] = useState(false);
  const [dChecked, setDChecked] = useState(false);
  const [dateFilter, setDateFilter] = useState("");
  const [merkFilter, setMerkFilter] = useState("");
  const [naamFilter, setNaamFilter] = useState("");
  const [sidebarVisible, setSidebarVisible] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      const storedUser = getUser();
      if (!storedUser) {
        setLoading(false);
        return;
      }
      setUser(storedUser);
      setLoading(false);

const productenInladen = () => {
  const filtered = products.filter(product => {
    if (
      (!aankomendChecked && product.status === "Aankomend") ||
      (!eigenChecked && product.status === "Eigen") ||
      (!gekochtChecked && product.status === "Verkocht") ||
      (!inTePlannenChecked && product.status === "In te plannen")
    ) return false

    fetchData();
  }, []);

  const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, checked } = e.target;
    if (id === "Aankomende producten") setAankomendChecked(checked);
    if (id === "Eigen producten") setEigenChecked(checked);
    if (id === "Gekochte producten") setGekochtChecked(checked);
    if (id === "In te plannen producten") setInTePlannenChecked(checked);
    if (id === "A") setAChecked(checked);
    if (id === "B") setBChecked(checked);
    if (id === "C") setCChecked(checked);
    if (id === "D") setDChecked(checked);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    if (id === "datum-input") setDateFilter(value);
    if (id === "merk-input") setMerkFilter(value);
    if (id === "naam-input") setNaamFilter(value);
  };

  const productenInladen = () =>
    products
      .filter(product => {
        if (
          (!aankomendChecked && product.status === "Aankomend") ||
          (!eigenChecked && product.status === "Eigen") ||
          (!gekochtChecked && product.status === "Verkocht") ||
          (!inTePlannenChecked && product.status === "In te plannen")
        ) return false;

    return true;
  });

  const renderCard = (product: any) => {
    // Prefer explicit checks for both the 'Type' and 'status' fields because the API might use either.
    const isGekocht = (product.type && product.type.toString().toLowerCase() === 'gekocht');
    const isEigen =  (product.type && product.type.toString().toLowerCase() === 'eigen');

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

  return filtered.map(renderCard);
};

        if (dateFilter && product.datum !== dateFilter) return false;
        if (merkFilter && !product.merk.toLowerCase().includes(merkFilter.toLowerCase())) return false;
        if (naamFilter && !product.naam.toLowerCase().includes(naamFilter.toLowerCase())) return false;

        return true;
      })
      .map(product => (
        <ProductCard
          key={product.id}
          naam={product.naam}
          merk={product.merk}
          prijs={product.prijs}
          datum={product.datum}
          locatie={product.locatie}
          status={product.status}
          Aantal={product.aantal}
        />
      ));

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
            {[{ name: 'Naaldwijk', key: 'a' }, { name: 'Aalsmeer', key: 'b' }, { name: 'Rijnsburg', key: 'c' }, { name: 'Eelde', key: 'd' }].map(({ name, key }) => (
              <a key={name} href={`/veiling?loc=${key}`} className="card">
                <p>Locatie {name}</p>
                <img src={`http://localhost:5156/images/locatie-${key}.jpg`} alt="" />
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
