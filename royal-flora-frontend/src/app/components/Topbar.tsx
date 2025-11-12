import React, { useEffect, useState } from 'react';
import '../../styles/Topbar.css';
import Sidebar from './Sidebar';
import { useRouter } from 'next/navigation';

interface TopbarProps {
    useSideBar?: boolean;
    currentPage: string;
    currentUser?: string;
}

const Topbar: React.FC<TopbarProps> = ({
    useSideBar = false,
    currentPage,
    currentUser
}) => {
    const router = useRouter();
    const [sidebarVisible, setSidebarVisible] = useState(true);
    
    
    const toggleSidebar = () => {
        setSidebarVisible(!sidebarVisible);
    };

    const [aankomendChecked, setAankomendChecked] = useState(true);
      const [eigenChecked, setEigenChecked] = useState(true);
      const [gekochtChecked, setGekochtChecked] = useState(true);
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

    return (
        <>
            <div className="topbar">
                <nav className="nav">
                    <div className="left">
                        {useSideBar && <div className="hamburger" onClick={toggleSidebar}>
                            <span></span>
                            <span></span>
                            <span></span>
                        </div>}
                        <span className="nav-text">{currentPage}</span>
                    </div>
                    
                    <div className="nav-logo-container">
                        <a href="/homepage" className="nav-logo-link" aria-label="Ga naar homepagina" onClick={() => router.push('/homepage')}>
                            <img
                                src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/Royal_FloraHolland_Logo.svg/1200px-Royal_FloraHolland_Logo.svg.png"
                                alt="Royal FloraHolland Logo"
                                className="nav-logo"
                            />
                        </a>
                    </div>
                    
                    <a className="pfp-container" href="/accountDetails" aria-label="Ga naar account details" onClick={() => router.push('/accountDetails')}>
                        <img
                            src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png"
                            alt="Profiel"
                            className="pfp-img"
                        />
                    </a>
                </nav>
            </div>
            {useSideBar && <Sidebar
                sidebarVisible={sidebarVisible}
                aankomendChecked={aankomendChecked}
                eigenChecked={eigenChecked}
                gekochtChecked={gekochtChecked}
                aChecked={aChecked}
                bChecked={bChecked}
                cChecked={cChecked}
                dChecked={dChecked}
                dateFilter={dateFilter}
                merkFilter={merkFilter}
                naamFilter={naamFilter}
                onCheckboxChange={handleCheckboxChange}
                onInputChange={handleInputChange}
            />}

        </>
    );
};export default Topbar;