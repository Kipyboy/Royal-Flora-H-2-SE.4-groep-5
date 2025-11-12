'use client'
import React, { useState } from 'react';    
import '../../styles/AccountDetails.css';
import { useRouter } from 'next/navigation';
import Topbar from '../components/Topbar';

interface UserDetails {
    voornaam: string;
    achternaam: string;
    email: string;
    telefoon: string;
    wachtwoord: string;
}

const AccountDetails: React.FC = () => {
    const [userDetails, setUserDetails] = useState<UserDetails>({
        voornaam: '',
        achternaam: '',
        email: '',
        telefoon: '',
        wachtwoord: ''
    });

    const [disabledFields, setDisabledFields] = useState<Record<string, boolean>>({
        voornaam: true,
        achternaam: true,
        email: true,
        telefoon: true,
        wachtwoord: true
    });

    const [errors, setErrors] = useState<Record<string, string>>({});

    const handleInputChange = (field: keyof UserDetails) => (e: React.ChangeEvent<HTMLInputElement>) => {
        setUserDetails(prev => ({
            ...prev,
            [field]: e.target.value
        }));
    };

    const toggleField = (field: keyof UserDetails) => {
        if (disabledFields[field]) {
            setDisabledFields(prev => ({
                ...prev,
                [field]: false
            }));
        } else {
            handleSave(field);
        }
    };

    const handleSave = async (field: keyof UserDetails) => {
        try {
            // data opslag hier
            
            setDisabledFields(prev => ({
                ...prev,
                [field]: true
            }));
            

            setErrors(prev => ({
                ...prev,
                [field]: ''
            }));
        } catch (error) {
            setErrors(prev => ({
                ...prev,
                [field]: 'Er ging iets mis bij het opslaan'
            }));
        }
    };

    const handleLogout = () => {
        // Implementeer logout logica
        window.location.href = '/';
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
            
            <Topbar
                currentPage="Account Details"
            />
            

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
                            <label htmlFor={field} className="label-text">{label}</label>
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