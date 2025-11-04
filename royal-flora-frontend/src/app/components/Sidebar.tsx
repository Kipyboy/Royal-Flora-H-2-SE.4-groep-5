import React from 'react';
import '../../styles/Sidebar.css';

interface SidebarProps {
    sidebarVisible: boolean;
}

const Sidebar: React.FC<SidebarProps> = ({
    sidebarVisible
}) => (

<div
          className="sidebar"
          style={{ display: sidebarVisible ? 'flex' : 'none' }}
        >
          <div className="toptext">
            <p>Filters</p>
          </div>
          <div
            className="filters"
          >
            <fieldset>
                <legend>Tonen:</legend>
                <div>
                    {['Aankomende producten', 'Eigen producten', 'Gekochte producten'].map((loc) => (
                        <div key={loc}>
                            <input type="checkbox" name={loc} id={loc}/>
                            <label htmlFor={loc}>{loc}</label>
                        </div>
                    ))}
                </div>
            </fieldset>
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
              <input type="date" id='datum-input'/>
            </fieldset>
            <fieldset>
              <legend>Merk</legend>
              <input type="text" id='merk-input'/>
            </fieldset>
            <fieldset>
                <legend>Naam</legend>
                <input type="text" id="naam-input"/>
            </fieldset>
          </div>
        </div>
);

export default Sidebar;