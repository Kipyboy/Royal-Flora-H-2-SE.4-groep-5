import React from 'react';
import '../../styles/Topbar.css';
import Sidebar from './Sidebar';
import { useRouter } from 'next/navigation';

interface TopbarProps {
    useSideBar: boolean;
    currentPage: string;
    currentUser?: string;
}

const Topbar: React.FC<TopbarProps> = ({
    useSideBar = false,
    currentPage,
    currentUser
}) => {
    const router = useRouter();

    return (
        <>
            <div className="topbar">
                <nav className="nav">
                    <div className="left">
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
                    
                    <a className="pfp-container" href="#">
                        <img
                            src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png"
                            alt="Profiel"
                            className="pfp-img"
                        />
                    </a>
                </nav>
            </div>

        </>
    );
};export default Topbar;