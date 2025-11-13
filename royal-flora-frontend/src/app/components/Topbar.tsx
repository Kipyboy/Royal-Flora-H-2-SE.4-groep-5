import React, { useEffect, useState } from 'react';
import '../../styles/Topbar.css';
import Sidebar from './Sidebar';
import { useRouter } from 'next/navigation';

interface TopbarProps {
    useSideBar?: boolean;
    currentPage: string;
    currentUser?: string;


    sidebarVisible?: boolean;
    toggleSidebar?: () => void;
    aankomendChecked?: boolean;
    eigenChecked?: boolean;
    gekochtChecked?: boolean;
    aChecked?: boolean;
    bChecked?: boolean;
    cChecked?: boolean;
    dChecked?: boolean;
    dateFilter?: string;
    merkFilter?: string;
    naamFilter?: string;
    onCheckboxChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onInputChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const Topbar: React.FC<TopbarProps> = ({
    useSideBar = false,
    currentPage,
    currentUser,
    sidebarVisible,
    toggleSidebar,
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
}) => {
    const router = useRouter();

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
            {useSideBar && sidebarVisible !== undefined && onCheckboxChange && onInputChange && (
            <Sidebar
                sidebarVisible={sidebarVisible}
                aankomendChecked={!!aankomendChecked}
                eigenChecked={!!eigenChecked}
                gekochtChecked={!!gekochtChecked}
                aChecked={!!aChecked}
                bChecked={!!bChecked}
                cChecked={!!cChecked}
                dChecked={!!dChecked}
                dateFilter={dateFilter || ""}
                merkFilter={merkFilter || ""}
                naamFilter={naamFilter || ""}
                onCheckboxChange={onCheckboxChange}
                onInputChange={onInputChange}
            />
            )}

        </>
    );
};export default Topbar;