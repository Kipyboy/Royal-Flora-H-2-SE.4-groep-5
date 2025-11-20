"use client";

import React, { useEffect, useState } from 'react';
import '../../styles/Topbar.css';
import Sidebar from './Sidebar';
import { useRouter } from 'next/navigation';
import { getSessionData } from '../utils/sessionService';
import AanvoerderSidebar from './AanvoerderSidebar';
import KlantSidebar from './KlantSidebar';
import VeilingmeesterSidebar from './VeilingmeesterSidebar';





interface TopbarProps {
    useSideBar?: boolean;
    currentPage: string;
    userRole?: string;

    sidebarVisible?: boolean;
    toggleSidebar?: () => void;
    aankomendChecked?: boolean;
    eigenChecked?: boolean;
    gekochtChecked?: boolean;
    inTePlannenChecked?: boolean;
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
    sidebarVisible,
    toggleSidebar,
    aankomendChecked,
    eigenChecked,
    gekochtChecked,
    inTePlannenChecked,
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
    const [userRole, setUserRole] = useState<string | undefined>(undefined);

    useEffect(() => {
        async function fetchSession() {
            const session = await getSessionData();
            if (session && session.userInfo && session.userInfo.Role) {
                setUserRole(session.userInfo.Role);
            }
        }
        fetchSession();
    }, []);

    const router = useRouter();
    const [dropdownVisible, setDropdownVisible] = useState(false);

    const toggleDropdown = (e: React.MouseEvent) => {
        e.stopPropagation();
        setDropdownVisible(!dropdownVisible);
    };

    const handleLogout = async () => {
        await fetch('http://localhost:5156/api/auth/logout', {
            method: 'POST',
            credentials: 'include',
            headers: {  
                'Content-Type': 'application/json',
            }
        });
        setDropdownVisible(false);
        router.push('/login');
    };

    // Close dropdown when clicking outside
    React.useEffect(() => {
        const handleClickOutside = () => {
            if (dropdownVisible) {
                setDropdownVisible(false);
            }
        };

        document.addEventListener('click', handleClickOutside);
        return () => {
            document.removeEventListener('click', handleClickOutside);
        };
    }, [dropdownVisible]);

    React.useEffect(() => {
            const fetchSessionData = async () => {
                try {
                    const response = await fetch('http://localhost:5156/api/auth/session', {
                        method: 'GET',
                        credentials: 'include',
                        headers: {
                            'Content-Type': 'application/json'
                        }
                    });
                    console
    
                    if (response.ok) {
                        const data = await response.json();    
                        setUserRole(data.role);
                    } else {
                        // Not logged in, redirect to login
                        console.error('Not logged in');
                    }
                } catch (error) {
                    console.error('Error fetching session:', error);
                }
            };
    
            fetchSessionData();
        }, []);

    return (
            <>
                <div className="topbar">
                    <nav className="nav">
                        <div className="left">
                            {useSideBar && (
                                <div className="hamburger" onClick={toggleSidebar}>
                                    <span></span>
                                    <span></span>
                                    <span></span>
                                </div>
                            )}
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
                        
                        <div className="pfp-container" onClick={toggleDropdown}>
                            <img
                                alt="Profiel"
                                src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png"
                                className="pfp-img"
                                aria-label="Account menu"
                            />
                            {dropdownVisible && (
                                <div className="dropdown-menu" onClick={(e) => e.stopPropagation()}>
                                    <button onClick={() => { setDropdownVisible(false); router.push('/accountDetails'); }}>
                                        Account Details
                                    </button>
                                    {userRole === 'Aanvoerder' && (
                                        <button onClick={() => { setDropdownVisible(false); router.push('/productRegistratieAanvoerder'); }}>
                                            Product registreren
                                        </button>
                                    )}
                                    <button className='logoutButton' onClick={handleLogout}>
                                        Uitloggen
                                    </button>
                                </div>
                            )}
                        </div>
                    </nav>
                </div>
                {useSideBar && sidebarVisible !== undefined && onCheckboxChange && onInputChange && (
                    <Sidebar
                        sidebarVisible={!!sidebarVisible}
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
                {userRole === "aanvoerder" && (
                    <AanvoerderSidebar
                        sidebarVisible={!!sidebarVisible}
                        aankomendChecked={!!aankomendChecked}
                        eigenChecked={!!eigenChecked}
                        aChecked={!!aChecked}
                        bChecked={!!bChecked}
                        cChecked={!!cChecked}
                        dChecked={!!dChecked}
                        dateFilter={dateFilter || ""}
                        merkFilter={merkFilter || ""}
                        naamFilter={naamFilter || ""}
                        onCheckboxChange={onCheckboxChange ?? (() => {})}
                        onInputChange={onInputChange ?? (() => {})}
                    />
                )}
                {userRole === "klant" && (
                    <KlantSidebar
                        sidebarVisible={!!sidebarVisible}
                        aankomendChecked={!!aankomendChecked}
                        gekochtChecked={!!gekochtChecked}
                        aChecked={!!aChecked}
                        bChecked={!!bChecked}
                        cChecked={!!cChecked}
                        dChecked={!!dChecked}
                        dateFilter={dateFilter || ""}
                        merkFilter={merkFilter || ""}
                        naamFilter={naamFilter || ""}
                        onCheckboxChange={onCheckboxChange ?? (() => {})}
                        onInputChange={onInputChange ?? (() => {})}
                    />
                )}
                {userRole === "veilingmeester" && (
                    <VeilingmeesterSidebar
                        sidebarVisible={!!sidebarVisible}
                        aankomendChecked={!!aankomendChecked}
                        inTePlannenChecked={!!inTePlannenChecked}
                        aChecked={!!aChecked}
                        bChecked={!!bChecked}
                        cChecked={!!cChecked}
                        dChecked={!!dChecked}
                        dateFilter={dateFilter || ""}
                        merkFilter={merkFilter || ""}
                        naamFilter={naamFilter || ""}
                        onCheckboxChange={onCheckboxChange ?? (() => {})}
                        onInputChange={onInputChange ?? (() => {})}
                    />
                )}
                
            </>
        );
    };
    export default Topbar;