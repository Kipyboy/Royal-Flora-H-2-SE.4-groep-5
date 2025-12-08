'use client'
import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/AccountDetails.css';
import Topbar from '../components/Topbar';
import { getSessionData } from '../utils/sessionService';
import { logout as clearAuth, getAuthHeaders } from '../utils/auth';

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
    const router = useRouter();
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

    React.useEffect(() => {
        const fetchSessionData = async () => {
            try {
                const authHeaders = getAuthHeaders();
                
                // Check if we have a token
                if (!authHeaders.Authorization) {
                    console.error('No token found');
                    router.push('/');
                    return;
                }

                const response = await fetch('http://localhost:5156/api/auth/allUserInfo', {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...authHeaders,
                    }
                });

                if (response.ok) {
                    const data = await response.json();
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
                    console.error('Failed to fetch session data');
                    router.push('/');
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
            const authHeaders = getAuthHeaders();
            
            if (!authHeaders.Authorization) {
                router.push('/');
                return;
            }
            
            if (field == 'wachtwoord') {
                            // At least 8 characters, at least one digit and one special character
            const pwdRegex = /^(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]).{8,}$/;
            if (!pwdRegex.test(userDetails[field])) {
                return setErrors(prev => ({
                    ...prev,
                    [field]: 'Wachtwoord moet minstens 8 tekens bevatten, inclusief een cijfer en een speciaal teken'
                }));
            }
            }
            // data opslag hier
            const response = await fetch('http://localhost:5156/api/auth/updateUserInfo', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        ...authHeaders
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

    const handleLogout = () => {
        clearAuth();
        router.push('/');
    };

    const handleDeleteAccount = async () => {
        if (window.confirm('Weet je zeker dat je je account wilt verwijderen?')) {
            try{
                const authHeaders = getAuthHeaders();
                const response = await fetch('http://localhost:5156/api/auth/deleteAccount', {
                        method: 'DELETE',
                        headers: {
                            'Content-Type': 'application/json',
                            ...authHeaders,
                        }
                    })
            clearAuth();
            console.log(response)
                router.push('/');
            } catch (error) {
                setErrors({ general: 'Kon account niet verwijderen' });
            }
        }   
    };

    

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