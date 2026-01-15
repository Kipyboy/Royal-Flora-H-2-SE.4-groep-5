'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/ProductRegistratieAanvoerder.css';
import Topbar from '../components/Topbar';
import { authFetch } from '../utils/api';
import { getUser } from '../utils/auth';
import { API_BASE_URL } from '../config/api';

interface ProductFormData {
  name: string;
  clockLocation: string;
  auctionDate: string;
  amount: string;
  minimumPrice: string;
  description: string;
  images: File | null;
}

interface FormErrors {
  name: string;
  clockLocation: string;
  auctionDate: string;
  amount: string;
  minimumPrice: string;
  description: string;
  image: string;
}

interface User {
  username: string;
  email: string;
  role: string;
  KVK?: string;
}

// Pagina voor Aanvoerders om producten te registreren voor de veiling.
// Valideert invoer (naam, datum, prijs, aantal, afbeelding), zet data in een FormData-object
// en verstuurt dit naar het backend endpoint. Alleen gebruikers met de rol 'Aanvoerder'
// mogen deze pagina gebruiken (anders redirect naar homepage).
export default function ProductRegistratieAanvoerderPage() {
  const router = useRouter();
  // State:
  // - user: huidige ingelogde gebruiker (gecontroleerd op rol Aanvoerder)
  // - formData: formulierwaarden (inclusief optionele afbeelding als File)
  // - errors: validatiefouten per veld
  // - isSubmitting: blokkeert de submit-knop tijdens versturen
  // - loading: toont tijdelijke laadstatus tijdens initialisatie
  const [user, setUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<ProductFormData>({
    name: '',
    clockLocation: '',
    auctionDate: '',
    amount: '',
    minimumPrice: '',
    description: '',
    images: null,
  });
  const [errors, setErrors] = useState<FormErrors>({
    name:'', clockLocation:'', auctionDate:'', amount:'', minimumPrice:'', description:'', image:''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loading, setLoading] = useState(true);

  // Bij mount: controleer of de gebruiker is ingelogd en de rol 'Aanvoerder' heeft.
  // Anders redirecten we naar login of homepage om toegang te voorkomen.
  useEffect(() => {
    const currentUser = getUser();
    if (!currentUser) {
      router.push('/login');
      return;
    }
    if (currentUser.role !== 'Aanvoerder') {
      router.push('/homepage');
      return;
    }
    setUser(currentUser);
    setLoading(false);
  }, [router]);

  // Algemene input handler voor tekst/select/prijsvelden.
  // Voor prijsvelden normaliseren we komma naar dot en verwijderen we ongeldige tekens
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    
    // Normalisatie voor prijsvelden: alleen cijfers, komma of punt toegestaan
    let processedValue = value;
    if (name === 'minimumPrice' && value) {
      // Alleen cijfers, komma en punt toestaan
      processedValue = value.replace(/[^0-9.,]/g, '');
      // Vervang komma door punt voor consistente decimaalscheiding
      processedValue = processedValue.replace(',', '.');
      // Voorkom meerdere punten
      const parts = processedValue.split('.');
      if (parts.length > 2) {
        processedValue = parts[0] + '.' + parts.slice(1).join('');
      }
    }
    
    // Update formulier state en clear eventuele fouten voor dit veld
    setFormData(prev => ({ ...prev, [name]: processedValue }));
    setErrors(prev => ({ ...prev, [name]: '' }));
  };

  // Afbeelding upload handler: controleert bestandsformaat en grootte (max 5MB).
  // Bij invalid bestand tonen we een foutmelding en resetten we de input.
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;
    const file = files[0];
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];

    if (!validTypes.includes(file.type)) {
      setErrors(prev => ({ ...prev, image: 'Alleen afbeeldingen (JPEG, PNG, GIF) zijn toegestaan' }));
      if (e.target) e.target.value = '';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      setErrors(prev => ({ ...prev, image: 'Elk bestand mag maximaal 5MB zijn' }));
      if (e.target) e.target.value = '';
      return;
    }

    // Sla het File-object op in state en clear eventuele foutmelding
    setFormData(prev => ({ ...prev, images: file }));
    setErrors(prev => ({ ...prev, image: '' }));

    // Reset de input value zodat hetzelfde bestand opnieuw geselecteerd kan worden indien nodig
    if (e.target) e.target.value = '';
  };

  // Verwijder geselecteerde afbeelding uit het formulier (gebruikersactie op preview)
  const removeImage = () => {
    setFormData(prev => ({ ...prev, images: null }));
  };

  // Voer client-side validatie uit voordat het formulier wordt verstuurd.
  // Controleert aanwezigheid en basisregels (datums in de toekomst, positief aantal/prijs, etc.)
  const validateForm = (): boolean => {
    const newErrors: FormErrors = { name:'', clockLocation:'', auctionDate:'', amount:'', minimumPrice:'', description:'', image:'' };
    let isValid = true;

    if (!formData.name.trim()) { newErrors.name='Product naam is verplicht'; isValid=false; }
    if (!formData.clockLocation) { newErrors.clockLocation='Klok locatie is verplicht'; isValid=false; }
    if (!formData.auctionDate) { newErrors.auctionDate='Veilingdatum is verplicht'; isValid=false; }
    else if(new Date(formData.auctionDate) < new Date(new Date().toDateString())){ newErrors.auctionDate='Veilingdatum moet in de toekomst liggen'; isValid=false; }
    if(!formData.amount || parseInt(formData.amount)<=0){ newErrors.amount='Aantal moet groter dan 0 zijn'; isValid=false; }
    if(!formData.minimumPrice || parseFloat(formData.minimumPrice)<0){ newErrors.minimumPrice='Minimum prijs moet positief zijn'; isValid=false; }
    if(!formData.description.trim() || formData.description.trim().length<10){ newErrors.description='Omschrijving moet minimaal 10 karakters zijn'; isValid=false; }

    setErrors(newErrors);
    return isValid;
  };

  // Verstuurt het formulier: normaliseer prijs, bouw een FormData object (voor multipart upload)
  // en roep het protected API endpoint aan via authFetch. Handelt serverfouten en netwerkfouten af.
  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if(!validateForm()) return;

      if(!user || !user.KVK){
      alert("Kan leverancier niet bepalen. Log opnieuw in.");
      router.push('/login');
      return;
    }

    setIsSubmitting(true);

    try{
      // Normaliseer de prijs (komma -> punt) en parseer naar float
      const normalizedPrice = formData.minimumPrice.replace(',', '.');
      const priceValue = parseFloat(normalizedPrice);
      
      if (isNaN(priceValue)) {
        alert('Ongeldig prijsformat');
        setIsSubmitting(false);
        return;
      }

      // Bouw multipart/form-data payload
      const submitData = new FormData();
      submitData.append('ProductNaam', formData.name);
      submitData.append('ProductBeschrijving', formData.description);
      submitData.append('MinimumPrijs', priceValue.toString()); // ✅ send as normalized float
      submitData.append('Locatie', formData.clockLocation);
      submitData.append('Datum', formData.auctionDate);
      submitData.append('Aantal', formData.amount);
      submitData.append('Leverancier', user.KVK);

      if (formData.images) submitData.append('images', formData.images);

      const response = await authFetch(`${API_BASE_URL}/api/products`, { method: 'POST', body: submitData });

      if(!response.ok){
        const error = await response.json();
        alert(`Registratie mislukt: ${error.message || 'Onbekende fout'}`);
        return;
      }

      alert('Product succesvol geregistreerd!');
      // Reset formulier state na succes
      setFormData({ name:'', clockLocation:'', auctionDate:'', amount:'', minimumPrice:'', description:'', images:null });

      const fileInput = document.getElementById('image') as HTMLInputElement;
      if(fileInput) fileInput.value='';
    } catch(err){
      console.error(err);
      alert('Er is een fout opgetreden bij het registreren van het product');
    } finally{
      setIsSubmitting(false);
    }
  };

  // Toon korte meldingen wanneer we nog initialiseren of de gebruiker niet (meer) gevonden is
  if(loading) return <p>Loading...</p>;
  if(!user) return <p>Geen gebruiker gevonden. Log opnieuw in.</p>;

  return (
    <div className="productRegistratieAanvoerder-page">
      <Topbar currentPage="Product registreren" />
      <div className="content">
        <form className="formContainer" onSubmit={handleSubmit}>
          <div className="groupContainer">
            <label htmlFor="name">Product naam:</label>
            <input
              id="name"
              name="name"
              type="text"
              value={formData.name}
              onChange={handleInputChange}
              required
            />
            {errors.name && <div className="error-message">{errors.name}</div>}
          </div>

          <div className="inlineGroup">
            <div className="groupContainer">
              <label htmlFor="clockLocation">Klok locatie:</label>
              <select
                id="clockLocation"
                name="clockLocation"
                value={formData.clockLocation}
                onChange={handleInputChange}
                required
              >
                <option value="">-- Selecteer locatie --</option>
                <option value="Naaldwijk">Naaldwijk</option>
                <option value="Aalsmeer">Aalsmeer</option>
                <option value="Rijnsburg">Rijnsburg</option>
                <option value="Eelde">Eelde</option>
              </select>
              {errors.clockLocation && <div className="error-message">{errors.clockLocation}</div>}
            </div>

            <div className="groupContainer">
              <label htmlFor="auctionDate">Veilingdatum:</label>
              <input
                id="auctionDate"
                name="auctionDate"
                type="date"
                value={formData.auctionDate}
                onChange={handleInputChange}
                required
              />
              {errors.auctionDate && <div className="error-message">{errors.auctionDate}</div>}
            </div>
          </div>

          <div className="inlineGroup">
            <div className="groupContainer">
              <label htmlFor="amount">Aantal:</label>
              <input
                id="amount"
                name="amount"
                type="number"
                min="1"
                value={formData.amount}
                onChange={handleInputChange}
                required
              />
              {errors.amount && <div className="error-message">{errors.amount}</div>}
            </div>

            <div className="groupContainer">
              <label htmlFor="minimumPrice">Minimum prijs (€):</label>
              <input
                id="minimumPrice"
                name="minimumPrice"
                type="text"
                placeholder="0.00"
                value={formData.minimumPrice}
                onChange={handleInputChange}
                required
              />
              {errors.minimumPrice && <div className="error-message">{errors.minimumPrice}</div>}
            </div>
          </div>

          <div className="groupContainer">
            <label htmlFor="description">Omschrijving:</label>
            <input
              id="description"
              name="description"
              type="text"
              className="bigInput"
              value={formData.description}
              onChange={handleInputChange}
              required
              minLength={10}
            />
            {errors.description && <div className="error-message">{errors.description}</div>}
          </div>

          <div className="groupContainer">
            <label htmlFor="image">Upload afbeelding:</label>
            {/* Bestandinput accepteert alleen afbeeldingstypes; validatie gebeurt in handleFileChange */}
            <input
              id="image"
              name="image"
              type="file"
              accept="image/jpeg,image/jpg,image/png,image/gif"
              onChange={handleFileChange}
            />
            {/* Preview van geselecteerde afbeelding met mogelijkheid om die te verwijderen */}
            {formData.images && (
              <div className="image-preview-container">
                <div className="image-preview-item">
                  <img src={URL.createObjectURL(formData.images)} alt={`Preview`} className="image-preview" />
                  <div className="image-preview-info">
                    <span className="image-name">{formData.images.name}</span>
                    <button type="button" className="remove-image-btn" onClick={() => removeImage()}>✕</button>
                  </div>
                </div>
              </div>
            )}
            {errors.image && <div className="error-message">{errors.image}</div>}
          </div>

          <div className="groupContainer">
            <input
              type="submit"
              className="submitButton"
              value={isSubmitting ? 'Registreren...' : 'Registreer Product'}
              disabled={isSubmitting}
            />
          </div>
        </form>
      </div>
    </div>
  );
}
