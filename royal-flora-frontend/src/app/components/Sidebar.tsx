import React from 'react';
import '../../styles/Sidebar.css';

interface SidebarProps {
    sidebarVisible: boolean;
    aankomendChecked: boolean;
    eigenChecked: boolean;
    gekochtChecked: boolean;
    aChecked: boolean;
    bChecked: boolean;
    cChecked: boolean;
    dChecked: boolean;
    dateFilter: string;
    merkFilter: string;
    naamFilter: string;
    onCheckboxChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onInputChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}



const Sidebar: React.FC<SidebarProps> = ({
    sidebarVisible,
    aankomendChecked,
    eigenChecked,
    gekochtChecked,
    aChecked,
    bChecked,
    cChecked,
    dChecked,
    dateFilter,
    merkFilter,
    naamFilter,
    onCheckboxChange,
    onInputChange
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
          <div>
            <input type="checkbox" name="Gekochte producten" id="Gekochte producten" 
            checked={gekochtChecked}
            onChange={onCheckboxChange}
            />
            <label htmlFor="Gekochte producten">Gekochte producten</label>
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
          </div>
        </div>
);

export default Sidebar;