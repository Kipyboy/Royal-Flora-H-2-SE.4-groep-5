'use client'
import React, { useState } from 'react';    
import '../../styles/AccountDetails.css';
import { useRouter } from 'next/navigation';

interface UserDetails {
    voornaam: string;
    achternaam: string;
    email: string;
    telefoon: string;
    wachtwoord: string;
}

const AccountDetails: React.FC = () => {
    // State voor gebruikersgegevens
    const [userDetails, setUserDetails] = useState<UserDetails>({
        voornaam: '',
        achternaam: '',
        email: '',
        telefoon: '',
        wachtwoord: ''
    });

    // State voor disabled velden
    const [disabledFields, setDisabledFields] = useState<Record<string, boolean>>({
        voornaam: true,
        achternaam: true,
        email: true,
        telefoon: true,
        wachtwoord: true
    });

    // State voor error messages
    const [errors, setErrors] = useState<Record<string, string>>({});

    const handleInputChange = (field: keyof UserDetails) => (e: React.ChangeEvent<HTMLInputElement>) => {
        setUserDetails(prev => ({
            ...prev,
            [field]: e.target.value
        }));
    };

    const toggleField = (field: keyof UserDetails) => {
        if (disabledFields[field]) {
            // Veld wordt enabled
            setDisabledFields(prev => ({
                ...prev,
                [field]: false
            }));
        } else {
            // Veld wordt opgeslagen en disabled
            handleSave(field);
        }
    };

    const handleSave = async (field: keyof UserDetails) => {
        try {
            // Hier komt je API call om de data op te slaan
            // await updateUserField(field, userDetails[field]);
            
            setDisabledFields(prev => ({
                ...prev,
                [field]: true
            }));
            
            // Clear error als het opslaan succesvol was
            setErrors(prev => ({
                ...prev,
                [field]: ''
            }));
        } catch (error) {
            // Toon error message als er iets mis gaat
            setErrors(prev => ({
                ...prev,
                [field]: 'Er ging iets mis bij het opslaan'
            }));
        }
    };

    const handleLogout = () => {
        // Implementeer logout logica
        window.location.href = '/login';
    };

    const handleDeleteAccount = () => {
        if (window.confirm('Weet je zeker dat je je account wilt verwijderen?')) {
            // Implementeer account verwijderen logica
            // await deleteAccount();
            window.location.href = '/login';
        }
    };

    const router = useRouter();

    return (
        <>
            <nav>
                <p className="nav-text">Account details</p>
                <a className="skip-link" onClick={() => router.push('/homepage')} href="/homepage">         
                    <img 
                        src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/Royal_FloraHolland_Logo.svg/1200px-Royal_FloraHolland_Logo.svg.png" 
                        alt="Royal FloraHolland Logo"
                    />
                </a>
                <a className="pfp-container" onClick={() => (router.push('/accountDetails'))} href="/accountDetails">
                    <img 
                        src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png" 
                        alt="Profiel foto" 
                    />
                </a>
            </nav>

            <main id="main">
                <form onSubmit={e => e.preventDefault()} aria-labelledby="accountdetails-title">
                    <h1 id="accountdetails-title">Account Details</h1>

                    {Object.entries({
                        voornaam: 'Voornaam',
                        achternaam: 'Achternaam',
                        email: 'Email adres',
                        telefoon: 'Telefoonnummer',
                        wachtwoord: 'Wachtwoord'
                    }).map(([field, label]) => (
                        <div className="form-group" key={field}>
                            <label htmlFor={field}>{label}</label>
                            <div className="input-row">
                                <input
                                    disabled={disabledFields[field]}
                                    type={field === 'wachtwoord' ? 'password' : 
                                          field === 'email' ? 'email' :
                                          field === 'telefoon' ? 'tel' : 'text'}
                                    id={field}
                                    name={field}
                                    value={userDetails[field as keyof UserDetails]}
                                    onChange={handleInputChange(field as keyof UserDetails)}
                                    aria-describedby={`${field}-error`}
                                    autoComplete={field === 'wachtwoord' ? 'new-password' : field}
                                />
                                <button
                                    type="button"
                                    className="field-button"
                                    aria-controls={field}
                                    onClick={() => toggleField(field as keyof UserDetails)}
                                >
                                    {disabledFields[field] ? 'Wijzig' : 'Opslaan'}
                                </button>
                            </div>
                            {errors[field] && (
                                <div id={`${field}-error`} className="error-message" aria-live="polite">
                                    {errors[field]}
                                </div>
                            )}
                        </div>
                    ))}

                    <button onClick={handleLogout} className="uitlog-button">
                        Uitloggen
                    </button>
                    <button onClick={handleDeleteAccount} className="verwijder-button">
                        Account verwijderen
                    </button>
                </form>
            </main>
        </>
    );
};

export default AccountDetails;