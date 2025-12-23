'use client';

import React, { useEffect, useState } from 'react';
import '../../styles/Topbar.css';
import Sidebar from './Sidebar';
import { useRouter } from 'next/navigation';
import { logout as clearAuth, getUser } from '../utils/auth';
import AanvoerderSidebar from './AanvoerderSidebar';
import KlantSidebar from './KlantSidebar';
import VeilingmeesterSidebar from './VeilingmeesterSidebar';
import { API_BASE_URL } from '../config/api';

interface TopbarProps {
    useSideBar?: boolean;
    currentPage: string;
    user?: {
        username: string;
        role: string;
        email?: string;
    };

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
    toonBeschrijving?: boolean;
    onCheckboxChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onInputChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onButtonClick?: React.MouseEventHandler<HTMLButtonElement>;
}

interface User {
    username: string;
    email?: string;
    role: string;
    KVK?: string;
}

const Topbar: React.FC<TopbarProps> = ({
    useSideBar = false,
    currentPage,
    user: userProp,
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
    toonBeschrijving,
    onCheckboxChange,
    onInputChange,
    onButtonClick
}) => {
    const [dropdownVisible, setDropdownVisible] = useState(false);
    const router = useRouter();
    const [user, setUser] = useState<User | null>(userProp ?? null);

    useEffect(() => {
        if (!userProp) {
            const fetched = getUser();
            setUser(fetched);
            console.log("Fetched user in Topbar:", fetched);
        }
    }, [userProp]);

    const toggleDropdown = (e: React.MouseEvent) => {
        e.stopPropagation();
        setDropdownVisible(!dropdownVisible);
    };

    const handleLogout = async () => {
        try {
            await fetch(`${API_BASE_URL}/api/auth/logout`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });
        } catch (err) {
            // ignore
        }
        clearAuth();
        setDropdownVisible(false);
        router.push('/');
    };

    // Close dropdown when clicking outside
    useEffect(() => {
        const handleClickOutside = () => {
            if (dropdownVisible) setDropdownVisible(false);
        };
        document.addEventListener('click', handleClickOutside);
        return () => document.removeEventListener('click', handleClickOutside);
    }, [dropdownVisible]);

    console.log("Topbar rendered with user:", user);

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
                                src={`${API_BASE_URL}/images/toppng.com-flower-png-tumblr-flowers-clear-background-flower-500x489.png`}
                                alt="Royal FloraHolland Logo"
                                className="nav-logo"
                            />
                        </a>
                    </div>

                    
                        {user?.role === 'Aanvoerder' && (
                            <div className="role-specific-nav-text">
                            <a onClick={() => { setDropdownVisible(false); router.push('/productRegistratieAanvoerder'); }}>
                                <p>Product registreren</p>
                            </a>
                            </div>
                        )}
                    

                    <div className="pfp-container" onClick={toggleDropdown}>
                        <img
                            alt="Profiel"
                            src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png"
                            className="pfp-img"
                            aria-label="Account menu"
                        />

                        {dropdownVisible && (
                            <div className="dropdown-menu" onClick={(e) => e.stopPropagation()}>
                                <button
                                    onClick={() => {
                                        setDropdownVisible(false);
                                        router.push('/accountDetails');
                                    }}
                                >
                                    Account Details
                                </button>
                                <button
                                    onClick={() => {
                                        setDropdownVisible(false);
                                        router.push('/bedrijfDetails');
                                    }}
                                >
                                    Bedrijf Details
                                </button>

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
                        dateFilter={dateFilter || ''}
                        merkFilter={merkFilter || ''}
                        naamFilter={naamFilter || ''}
                        onCheckboxChange={onCheckboxChange}
                            onInputChange={onInputChange}
                            onButtonClick={onButtonClick}
                    />
                )}

            {user?.role === 'Aanvoerder' && (
                <AanvoerderSidebar
                    sidebarVisible={!!sidebarVisible}
                    aankomendChecked={!!aankomendChecked}
                    eigenChecked={!!eigenChecked}
                    aChecked={!!aChecked}
                    bChecked={!!bChecked}
                    cChecked={!!cChecked}
                    dChecked={!!dChecked}
                    dateFilter={dateFilter || ''}
                    merkFilter={merkFilter || ''}
                    naamFilter={naamFilter || ''}
                    toonBeschrijving={toonBeschrijving || false}
                    onCheckboxChange={onCheckboxChange ?? (() => {})}
                    onInputChange={onInputChange ?? (() => {})}
                    onButtonClick={onButtonClick}
                />
            )}
            {user?.role === 'Inkoper' && (
                <KlantSidebar
                    sidebarVisible={!!sidebarVisible}
                    aankomendChecked={!!aankomendChecked}
                    gekochtChecked={!!gekochtChecked}
                    aChecked={!!aChecked}
                    bChecked={!!bChecked}
                    cChecked={!!cChecked}
                    dChecked={!!dChecked}
                    dateFilter={dateFilter || ''}
                    merkFilter={merkFilter || ''}
                    naamFilter={naamFilter || ''}
                    toonBeschrijving={toonBeschrijving || false}
                    onCheckboxChange={onCheckboxChange ?? (() => {})}
                    onInputChange={onInputChange ?? (() => {})}
                    onButtonClick={onButtonClick}
                />
            )}
            {user?.role === 'Veilingmeester' && (
                <VeilingmeesterSidebar
                    sidebarVisible={!!sidebarVisible}
                    aankomendChecked={!!aankomendChecked}
                    inTePlannenChecked={!!inTePlannenChecked}
                    aChecked={!!aChecked}
                    bChecked={!!bChecked}
                    cChecked={!!cChecked}
                    dChecked={!!dChecked}
                    dateFilter={dateFilter || ''}
                    merkFilter={merkFilter || ''}
                    naamFilter={naamFilter || ''}
                    toonBeschrijving={toonBeschrijving || false}
                    onCheckboxChange={onCheckboxChange ?? (() => {})}
                    onInputChange={onInputChange ?? (() => {})}
                    onButtonClick={onButtonClick}
                />
            )}
        </>
    );
};

export default Topbar;
