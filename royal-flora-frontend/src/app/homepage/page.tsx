"use client"

import React, { useState } from 'react';
import Head from 'next/head';
import Sidebar from '../components/Sidebar';
import ProductCard from '../components/product-card'
import  '../../styles/homepage.css';



const HomePage: React.FC = () => {
  const [sidebarVisible, setSidebarVisible] = useState(true);


  const toggleSidebar = () => {
    setSidebarVisible(!sidebarVisible);
  };


  return (
    <>
      <Head>
        <title>Home - Royal FloraHolland</title>
        <meta charSet="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      </Head>

  <nav className="nav">
    <div className="left">
      <div className="hamburger" onClick={toggleSidebar}>
        <span></span>
        <span></span>
        <span></span>
      </div>
      <span className="nav-text">Home</span>
      </div>
    <div className="nav-logo-container">
      <img
        src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/Royal_FloraHolland_Logo.svg/1200px-Royal_FloraHolland_Logo.svg.png"
        alt="Royal FloraHolland Logo"
        className="nav-logo"
      />
    </div>
    <a className="pfp-container" href="/productRegistratieAanvoerder">
      <img
        src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png"
        alt="Profiel"
        className="pfp-img"
      />
    </a>
  </nav>
  
  <div className="main-layout">
  <Sidebar
    sidebarVisible={sidebarVisible}
    />

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
            <ProductCard/>
          </div>
        </div>
      </div>
    </>
  );
};

export default HomePage;
``