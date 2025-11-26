'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/Registreren.css';

// -------------------- TYPE DEFINITIONS --------------------
type FormDataType = {
  voornaam: string;
  achternaam: string;
  telefoon: string;
  email: string;
  password: string;
  confirmPassword: string;
  kvk: string;
  accountType: 'klant' | 'bedrijf';
  postcode: string;
  adres: string;
};

export default function Registreren() {
  const router = useRouter();

  // -------------------- STATE --------------------
  const [formData, setFormData] = useState<FormDataType>({
    voornaam: '',
    achternaam: '',
    telefoon: '',
    email: '',
    password: '',
    confirmPassword: '',
    kvk: '',
    accountType: 'klant',
    postcode: '',
    adres: ''
  });

  const [errors, setErrors] = useState<Omit<FormDataType, 'accountType'>>({
    voornaam: '',
    achternaam: '',
    telefoon: '',
    email: '',
    password: '',
    confirmPassword: '',
    kvk: '',
    postcode: '',
    adres: ''
  });

  // -------------------- HANDLE CHANGE --------------------
  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;

    setFormData(prev => ({
      ...prev,
      [name as keyof FormDataType]: value
    }));

    setErrors(prev => ({
      ...prev,
      [name as keyof typeof prev]: ''
    }));
  };

  // -------------------- FORM VALIDATION --------------------
  const validateForm = () => {
    const newErrors: typeof errors = {
      voornaam: '',
      achternaam: '',
      telefoon: '',
      email: '',
      password: '',
      confirmPassword: '',
      kvk: '',
      postcode: '',
      adres: ''
    };
    let isValid = true;

    if (!formData.voornaam.trim()) {
      newErrors.voornaam = 'Voornaam is verplicht';
      isValid = false;
    }

    if (!formData.achternaam.trim()) {
      newErrors.achternaam = 'Achternaam is verplicht';
      isValid = false;
    }

    if (!formData.email.trim()) {
      newErrors.email = 'Email is verplicht';
      isValid = false;
    }

    if (!formData.password) {
      newErrors.password = 'Wachtwoord is verplicht';
      isValid = false;
    } else if (formData.password.length < 6) {
      newErrors.password = 'Wachtwoord moet minimaal 6 karakters zijn';
      isValid = false;
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Herhaal wachtwoord is verplicht';
      isValid = false;
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Wachtwoorden komen niet overeen';
      isValid = false;
    }

    if (!formData.kvk.trim()) {
      newErrors.kvk = 'KvK-nummer is verplicht';
      isValid = false;
    } else if (!/^\d{8}$/.test(formData.kvk)) {
      newErrors.kvk = 'KvK-nummer moet 8 cijfers bevatten';
      isValid = false;
    }

    if (!formData.postcode.trim()) {
      newErrors.postcode = 'Postcode is verplicht';
      isValid = false;
    }

    if (!formData.adres.trim()) {
      newErrors.adres = 'Adres is verplicht';
      isValid = false;
    }

    setErrors(newErrors);
    return isValid;
  };

  // -------------------- HANDLE SUBMIT --------------------
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!validateForm()) return;

    try {
      const response = await fetch('http://localhost:5156/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          voorNaam: formData.voornaam,
          achterNaam: formData.achternaam,
          telefoonnummer: formData.telefoon,
          e_mail: formData.email,
          wachtwoord: formData.password,
          kvkNummer: formData.kvk,
          postcode: formData.postcode,
          adres: formData.adres,
          accountType: formData.accountType
        })
      });

      const data = await response.json().catch(() => null);

      if (response.ok) {
        alert('Registratie succesvol!');
        const currentTimeInSeconds = Math.floor(Date.now() / 1000);

        if (currentTimeInSeconds % 2 === 0) {
          router.push('/login');
        } else {
          router.push('/homepage');
        }
      } else {
        alert(`Registratie mislukt: ${data?.message || 'Onbekende fout'}`);
      }
    } catch (error) {
      console.error('Error:', error);
      alert('Er is een fout opgetreden bij het registreren');
    }
  };

  // -------------------- RENDER --------------------
  return (
    <div className="registreren-page">
      <main id="main">
        <a href="#main" className="skip-link">Spring naar hoofdinhoud</a>

        <form onSubmit={handleSubmit} aria-labelledby="register-title">
          <h1 id="register-title">Registreren</h1>

          <fieldset className="name-row">
            <legend className="visually-hidden">Persoonlijke gegevens</legend>

            <div className="form-group half-width">
              <label htmlFor="voornaam">Voornaam</label>
              <input
                type="text"
                id="voornaam"
                name="voornaam"
                required
                value={formData.voornaam}
                onChange={handleChange}
                aria-describedby="voornaam-error"
                autoComplete="given-name"
              />
              {errors.voornaam && <div id="voornaam-error" className="error-message">{errors.voornaam}</div>}
            </div>

            <div className="form-group half-width">
              <label htmlFor="achternaam">Achternaam</label>
              <input
                type="text"
                id="achternaam"
                name="achternaam"
                required
                value={formData.achternaam}
                onChange={handleChange}
                aria-describedby="achternaam-error"
                autoComplete="family-name"
              />
              {errors.achternaam && <div id="achternaam-error" className="error-message">{errors.achternaam}</div>}
            </div>
          </fieldset>

          <div className="form-group">
            <label htmlFor="telefoon">Telefoonnummer (optioneel)</label>
            <input
              type="tel"
              id="telefoon"
              name="telefoon"
              value={formData.telefoon}
              onChange={handleChange}
              aria-describedby="telefoon-error"
              autoComplete="tel"
            />
            {errors.telefoon && <div id="telefoon-error" className="error-message">{errors.telefoon}</div>}
          </div>

          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              required
              value={formData.email}
              onChange={handleChange}
              aria-describedby="email-error"
              autoComplete="email"
            />
            {errors.email && <div id="email-error" className="error-message">{errors.email}</div>}
          </div>

          <div className="form-group">
            <label htmlFor="kvk">KvK-nummer</label>
            <input
              type="text"
              id="kvk"
              name="kvk"
              required
              value={formData.kvk}
              onChange={handleChange}
              aria-describedby="kvk-error"
            />
            {errors.kvk && <div id="kvk-error" className="error-message">{errors.kvk}</div>}
          </div>

          <fieldset>
            <div className="form-group">
              <label htmlFor="password">Wachtwoord</label>
              <input
                type="password"
                id="password"
                name="password"
                required
                value={formData.password}
                onChange={handleChange}
                aria-describedby="password-error"
              />
              {errors.password && <div id="password-error" className="error-message">{errors.password}</div>}
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword">Herhaal wachtwoord</label>
              <input
                type="password"
                id="confirmPassword"
                name="confirmPassword"
                required
                value={formData.confirmPassword}
                onChange={handleChange}
                aria-describedby="confirm-password-error"
              />
              {errors.confirmPassword && <div id="confirm-password-error" className="error-message">{errors.confirmPassword}</div>}
            </div>
          </fieldset>

          <div className="form-group">
            <label htmlFor="postcode">Postcode</label>
            <input
              type="text"
              id="postcode"
              name="postcode"
              required
              value={formData.postcode}
              onChange={handleChange}
            />
            {errors.postcode && <div className="error-message">{errors.postcode}</div>}
          </div>

          <div className="form-group">
            <label htmlFor="adres">Adres</label>
            <input
              type="text"
              id="adres"
              name="adres"
              required
              value={formData.adres}
              onChange={handleChange}
            />
            {errors.adres && <div className="error-message">{errors.adres}</div>}
          </div>

          <div className="form-group">
            <label htmlFor="accountType">Account type</label>
            <select
              id="accountType"
              name="accountType"
              value={formData.accountType}
              onChange={handleChange}
            >
              <option value="klant">Inkooper</option>
              <option value="bedrijf">Aanvoerder</option>
            </select>
          </div>

          <button type="submit" className="register-button">Registreren</button>
        </form>
      </main>
    </div>
  );
}
