import React from 'react';
import '../../styles/Sidebar.css';

// Sidebar voor de rol 'Aanvoerder'. Bevat filters (locatie, datum, merk, naam) en een
// toggle-knop om tussen productoverzicht en volledige beschrijvingen te wisselen.
// Alle waarden zijn controlled props; wijzigingen worden doorgegeven via callbacks.
interface SidebarProps {
  sidebarVisible: boolean; // bepaalt of het paneel zichtbaar is
  aankomendChecked: boolean; // filter: aankomende producten
  eigenChecked: boolean; // filter: eigen producten
  aChecked: boolean; // filter: locatie A
  bChecked: boolean; // filter: locatie B
  cChecked: boolean; // filter: locatie C
  dChecked: boolean; // filter: locatie D
  dateFilter: string; // geselecteerde datum
  merkFilter: string; // merk-filter tekst
  naamFilter: string; // naam-filter tekst
  toonBeschrijving: boolean; // togglet tussen overzicht/beschrijvingen
  onCheckboxChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onInputChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onButtonClick?: React.MouseEventHandler<HTMLButtonElement>;
}

const AanvoerderSidebar: React.FC<SidebarProps> = ({
  sidebarVisible,
  aankomendChecked,
  eigenChecked,
  aChecked,
  bChecked,
  cChecked,
  dChecked,
  dateFilter,
  merkFilter,
  naamFilter,
  toonBeschrijving,
  onCheckboxChange,
  onInputChange,
  onButtonClick
}) => (
  <div
    className="sidebar"
    style={{ display: sidebarVisible ? 'flex' : 'none' }}
  >
    <div className="toptext">
      <p>Filters</p>
    </div>

    <div className="filters">
      <fieldset>
        <legend>Tonen:</legend>
        <div>
          <div>
            <input type="checkbox" name="Aankomende producten" id="Aankomende producten"
              checked={aankomendChecked}
              onChange={onCheckboxChange}
            />
            <label htmlFor="Aankomende producten">Aankomende producten</label>
          </div>
          <div>
            <input type="checkbox" name="Eigen producten" id="Eigen producten"
              checked={eigenChecked}
              onChange={onCheckboxChange}
            />
            <label htmlFor="Eigen producten">Eigen producten</label>
          </div>
        </div>
      </fieldset>

      <fieldset>
        <legend>Locatie</legend>
        <div>
          <div>
            <input type="checkbox" name="A" id="A"
              checked={aChecked}
              onChange={onCheckboxChange}
            />
            <label htmlFor="A">Naaldwijk</label>
          </div>
          <div>
            <input type="checkbox" name="B" id="B"
              checked={bChecked}
              onChange={onCheckboxChange}
            />
            <label htmlFor="B">Aalsmeer</label>
          </div>
          <div>
            <input type="checkbox" name="C" id="C"
              checked={cChecked}
              onChange={onCheckboxChange}
            />
            <label htmlFor="C">Rijnsburg</label>
          </div>
          <div>
            <input type="checkbox" name="D" id="D"
              checked={dChecked}
              onChange={onCheckboxChange}
            />
            <label htmlFor="D">Eelde</label>
          </div>
        </div>
      </fieldset>

      <fieldset>
        <legend>Datum</legend>
        <input type="date" id='datum-input'
          value={dateFilter}
          onChange={onInputChange}
        />
      </fieldset>

      <fieldset>
        <legend>Merk</legend>
        <input type="text" id='merk-input'
          value={merkFilter}
          onChange={onInputChange}
        />
      </fieldset>

      <fieldset>
        <legend>Naam</legend>
        <input type="text" id="naam-input"
          value={naamFilter}
          onChange={onInputChange}
        />
      </fieldset>

      {/* Toggle tussen korte productoverzicht en volledige beschrijvingen */}
      <button className='description-button' onClick={onButtonClick}>
        <p>{toonBeschrijving ? 'Toon product overzicht' : 'Toon product beschrijvingen'}</p>
      </button>
    </div>

    <div className="sidebar-links">
      <a href="/status4products">Sold Products</a>
    </div>

  </div>
);

export default AanvoerderSidebar;