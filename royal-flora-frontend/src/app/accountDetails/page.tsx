'use client'
import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/AccountDetails.css';
import Topbar from '../components/Topbar';
import { getSessionData } from '../utils/sessionService';
import { logout as clearAuth } from '../utils/auth';

interface UserDetails {
    voornaam: string;
    achternaam: string;
    email: string;
    telefoonnummer: string;
    adress: string;
    postcode: string;
    wachtwoord: string;
}

const AccountDetails: React.FC = () => {
    const [userDetails, setUserDetails] = useState<UserDetails>({
        voornaam: '',
        achternaam: '',
        email: '',
        telefoonnummer: '',
        adress: '',
        postcode: '',
        wachtwoord: ''
    });

    const [disabledFields, setDisabledFields] = useState<Record<string, boolean>>({
        voornaam: true,
        achternaam: true,
        email: true,
        telefoonnummer: true,
        adress: true,
        postcode: true,
        wachtwoord: true
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [loading, setLoading] = useState<boolean>(true);

    // Fetch session data on component mount using bearer token
    React.useEffect(() => {
        const fetchSessionData = async () => {
            try {
                const data = await getSessionData();
                if (data) {
                    const nameParts = (data.username || '').split(' ');
                    const voornaam = nameParts[0] || '';
                    const achternaam = nameParts.slice(1).join(' ') || '';

                    setUserDetails({
                        voornaam: data.voorNaam,
                        achternaam: data.achterNaam,
                        email: data.email || '',
                        telefoonnummer: data.telefoonnummer || '',
                        adress: data.adress || '',
                        postcode: data.postcode || '',
                        wachtwoord: '' 
                    });
                } else {
                    // Not logged in, redirect to login
                    console.error('Not logged in');
                }
            } catch (error) {
                console.error('Error fetching session:', error);
                setErrors({ general: 'Kon sessie gegevens niet ophalen' });
            } finally {
                setLoading(false);
            }
        };

        fetchSessionData();
    }, []);


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
            console.log(`Saving field ${field} with value ${userDetails[field]}`);
            // data opslag hier
            const response = await fetch('http://localhost:5156/api/auth/updateUserInfo', {
                    method: 'POST',
                    credentials: 'include',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        Field: field,
                        Newvalue: userDetails[field]
                    })
                });
            
            if (response.ok) {
            
                setDisabledFields(prev => ({
                    ...prev,
                    [field]: true
                }));
                

                setErrors(prev => ({
                    ...prev,
                    [field]: ''
                }));
            } else {
                setErrors(prev => ({
                    ...prev,
                    [field]: 'Er ging iets mis bij het opslaan'
                }));
            }
        } catch (error) {
            setErrors(prev => ({
                ...prev,
                [field]: 'Er ging iets mis bij het opslaan'
            }));
        }
    };

    const handleLogout = async () => {
        try {
            await fetch('http://localhost:5156/api/auth/logout', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });
        } catch (err) {
            // ignore network errors on logout
        }
        clearAuth();
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

        if (loading) {
        return (
            <div className='accountDetails-page'>
                <Topbar
                    currentPage="Account Details"
                    useSideBar={false}
                />
                <main id="main">
                    <p>Laden...</p>
                </main>
            </div>
        );
    }

    return (
        <div className='accountDetails-page'>
            
            <Topbar
                currentPage="Account Details"
                useSideBar={false}
            />
            

            <main id="main">
                <form onSubmit={e => e.preventDefault()} aria-labelledby="accountdetails-title">
                    <h1 id="accountdetails-title">Account Details</h1>

                    {Object.entries({
                        voornaam: 'Voornaam',
                        achternaam: 'Achternaam',
                        email: 'Email adres',
                        telefoonnummer: 'Telefoonnummer',
                        adress: 'Adres',
                        postcode: 'Postcode',
                        wachtwoord: 'Wachtwoord'
                    }).map(([field, label]) => (
                        <div className="form-group" key={field}>
                            <label htmlFor={field} className="label-text">{label}</label>
                            <div className="input-row">
                                <input
                                    disabled={disabledFields[field]}
                                    type={field === 'wachtwoord' ? 'password' : 
                                          field === 'email' ? 'email' :
                                          field === 'telefoonnummer' ? 'tel' : 'text'}
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
        </div>
    );
};

export default AccountDetails;