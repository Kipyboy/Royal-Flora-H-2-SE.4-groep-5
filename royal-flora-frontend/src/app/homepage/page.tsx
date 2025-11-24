"use client"

import React, { useEffect, useState } from 'react';
import Head from 'next/head';
import Sidebar from '../components/Sidebar';
import ProductCard from '../components/product-card'
import  '../../styles/homepage.css';
import { mock } from 'node:test';
import Topbar from '../components/Topbar';



const HomePage: React.FC = () => {
  const [products, setProducts] = useState<any[]>([]);

  

  const getProducts = async () => {
      try {
        const response = await fetch("http://localhost:5156/api/products1");
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
  const [eigenChecked, setEigenChecked] = useState(true);
  const [gekochtChecked, setGekochtChecked] = useState(true);
  const [inTePlannenChecked, setInTePlannenChecked] = useState(true);
  const [aChecked, setAChecked] = useState(false);
  const [bChecked, setBChecked] = useState(false);
  const [cChecked, setCChecked] = useState(false);
  const [dChecked, setDChecked] = useState(false);
  const [dateFilter, setDateFilter] = useState("");
  const [merkFilter, setMerkFilter] = useState("");
  const [naamFilter, setNaamFilter] = useState("");

const handleCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, checked } = e.target;
    if (id == "Aankomende producten") setAankomendChecked(checked)
    if (id == "Eigen producten") setEigenChecked(checked)
    if (id == "Gekochte producten") setGekochtChecked(checked)
    if (id == "A") setAChecked(checked)
    if (id == "B") setBChecked(checked)
    if (id == "C") setCChecked(checked)
    if (id == "D") setDChecked(checked)
}
const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    if (id == "datum-input") setDateFilter(value);
    if (id == "merk-input") setMerkFilter(value);
    if (id == "naam-input") setNaamFilter(value);
}

const productenInladen = () => {
  return products
  .filter(product => {
    if (
      (!aankomendChecked && product.status === "Aankomend") ||
      (!eigenChecked && product.status === "Eigen") ||
      (!gekochtChecked && product.status === "Verkocht") ||
      (!inTePlannenChecked && product.status === "In te plannen")
    ) return false

    if (
      (
        (aChecked || bChecked || cChecked || dChecked) &&
        (
          (!aChecked && product.locatie === "A") ||
          (!bChecked && product.locatie === "B") ||
          (!cChecked && product.locatie === "C") ||
          (!dChecked && product.locatie === "D")
        )
      )
    ) return false

    if (dateFilter && product.datum !== dateFilter) return false;

    if(merkFilter && !product.merk.toLowerCase().includes(merkFilter.toLowerCase())) return false;

    if(naamFilter && !product.naam.toLowerCase().includes(naamFilter.toLowerCase())) return false;

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
      />
  ));
};

  const [sidebarVisible, setSidebarVisible] = useState(true);


  const toggleSidebar = () => {
    setSidebarVisible(!sidebarVisible);
  };


  return (
    <div className='homepage-page'>
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
            />
  
      <div className="main-layout">

    

      <div className="content">
          <div className="veilingen">
            {['A', 'B', 'C', 'D'].map((loc) => (
              <a key={loc} href="/veiling" className="card">
                <p>Locatie {loc}</p>
                <p>Aanvoerder:</p>
                <p>Verlopen tijd:</p>
                <p>Huidig product:</p>
              </a>
            ))}
          </div>

          <div className="producten">
            {productenInladen()}
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;
``