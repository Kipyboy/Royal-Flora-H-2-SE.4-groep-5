"use client"

import React, { useState } from 'react';
import '../../styles/homepage.css';

const HomePage: React.FC = () => {
  const [sidebarVisible, setSidebarVisible] = useState(true);
  const [showGekochteProducten, setShowGekochteProducten] = useState(false);

  const toggleSidebar = () => {
    setSidebarVisible(!sidebarVisible);
  };

  const handleVeranderPaginaClick = () => {
    setShowGekochteProducten((prev) => !prev);
  };

  return (
    <div className="homepage-page">
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
        <div
          className="sidebar"
          style={{ display: sidebarVisible ? 'none' : 'flex' }}
        >
          <div className="toptext">
            <p>Filters</p>
          </div>
          <button
            className="verander-pagina-content"
            onClick={handleVeranderPaginaClick}
          >
            {showGekochteProducten ? 'Toon aankomende producten' : 'Toon gekochte producten'}
          </button>
          <div
            className="filters"
            style={{ display: showGekochteProducten ? 'block' : 'none' }}
          >
            <fieldset>
              <legend>Locatie</legend>
              <div>
                {['A', 'B', 'C', 'D'].map((loc) => (
                  <div key={loc}>
                    <input type="checkbox" name={loc} id={loc} />
                    <label htmlFor={loc}>{loc}</label>
                  </div>
                ))}
              </div>
            </fieldset>
            <fieldset>
              <legend>Datum</legend>
              <input type="date" />
            </fieldset>
            <fieldset>
              <legend>Merk</legend>
              <input type="text" />
            </fieldset>
          </div>
        </div>

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
            <div className="aankomende-producten">
              <p>{showGekochteProducten ? 'Gekochte producten' : 'Aankomende producten per locatie:'}</p>
              <div className="aankomende-producten-lijst">
                {['A', 'B', 'C', 'D'].map((loc) => (
                  <div className="aankomende-producten-voor-locatie" key={loc}
                  style={{display: showGekochteProducten ? 'none' : 'block'}}>
                    <p>Locatie {loc}</p>
                    {[1, 2, 3, 4, 5].map((num) => (
                      <p key={num}>{num}</p>
                    ))}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;
``