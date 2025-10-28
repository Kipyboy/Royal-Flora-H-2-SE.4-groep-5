import React, { useState } from 'react';
import Head from 'next/head';
import styles from '../styles/homepage.module.css';
import rootStyles from '../styles/Root.module.css'; // Als je globale root styles hebt

const HomePage: React.FC = () => {
  const [sidebarVisible, setSidebarVisible] = useState(true);

  const toggleSidebar = () => {
    setSidebarVisible(!sidebarVisible);
  };

  const handleVeranderPaginaClick = () => {
    // Voeg hier je logica toe
    console.log('Toon gekochte producten');
  };

  return (
    <>
      <Head>
        <title>Home - Royal FloraHolland</title>
        <meta charSet="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      </Head>

      <nav className={styles.nav}>
        <div className={styles.hamburger} onClick={toggleSidebar}>
          <span></span>
          <span></span>
          <span></span>
        </div>
        <span className={styles.navText}>Home</span>
        <img
          src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/Royal_FloraHolland_Logo.svg/1200px-Royal_FloraHolland_Logo.svg.png"
          alt="Royal FloraHolland Logo"
        />
        <img
          src="https://www.kindpng.com/picc/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png"
          alt="Profiel"
          className={styles.profileIcon}
        />
      </nav>

      <div className={styles.mainLayout}>
        {sidebarVisible && (
          <div className={styles.sidebar}>
            <div className={styles.toptext}>
              <p>Filters</p>
            </div>
            <button
              className={styles.veranderPaginaContent}
              onClick={handleVeranderPaginaClick}
            >
              Toon gekochte producten
            </button>
            <div className={styles.filters}>
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
        )}

        <div className={styles.content}>
          <div className={styles.veilingen}>
            {['A', 'B', 'C', 'D'].map((loc) => (
              <a key={loc} href="#" className={styles.veilingLink}>
                <p>Locatie {loc}</p>
                <p>Aanvoerder:</p>
                <p>Verlopen tijd:</p>
                <p>Huidig product:</p>
              </a>
            ))}
          </div>

          <div className={styles.producten}>
            <div className={styles.aankomendeProducten}>
              <p>Aankomende producten per locatie:</p>
              <div className={styles.aankomendeProductenLijst}>
                {['A', 'B', 'C', 'D'].map((loc) => (
                  <div className={styles.aankomendeProductenVoorLocatie} key={loc}>
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
    </>
  );
};

export default HomePage;
``