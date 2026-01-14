'use client';
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import '../../styles/veilingMeesterProduct.css';
import Topbar from '../components/Topbar';
import { authFetch } from '../utils/api';
import { getUser, getAuthHeaders} from '../utils/auth';
import { API_BASE_URL } from '../config/api';

interface ProductFormData {
  auctionDate: string;
  startPrice: string;
}

interface FormErrors {
  auctionDate: string;
  startPrice: string;
}

interface User {
  username: string;
  email: string;
  role: string;
  KVK?: string;
}

interface ProductDTO {
  id: number;
  naam: string;
  beschrijving: string;
  merk: string;
  prijs?: number | null;
  verkoopPrijs?: number | null;
  koper?: string;
  datum: string;
  locatie: string;
  status: string;
  aantal?: number | null;
  fotoPath: string;
}

export default function VeilingMeesterProductPage() {
  const router = useRouter();
  const [currentProduct, setCurrentProduct] = useState<ProductDTO | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<ProductFormData>({
    auctionDate: '',
    startPrice: '',
  });
  const [errors, setErrors] = useState<FormErrors>({
    auctionDate:'', startPrice:''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loading, setLoading] = useState(true);
  const [products, setProducts] = useState<ProductDTO[]>([]);

  useEffect(() => {
    const currentUser = getUser();
    if (!currentUser) {
      router.push('/login');
      return;
    }
    if (currentUser.role !== 'Veilingmeester') {
      router.push('/homepage');
      return;
    }
    setUser(currentUser);
    setLoading(false);
  }, [router]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    
    // For price fields, normalize comma to dot
    let processedValue = value;
    if (name === 'startPrice' && value) {
      // Allow only digits, comma, and dot
      processedValue = value.replace(/[^0-9.,]/g, '');
      // Convert comma to dot for consistency
      processedValue = processedValue.replace(',', '.');
      // Prevent multiple dots
      const parts = processedValue.split('.');
      if (parts.length > 2) {
        processedValue = parts[0] + '.' + parts.slice(1).join('');
      }
    }
    
    setFormData(prev => ({ ...prev, [name]: processedValue }));
    setErrors(prev => ({ ...prev, [name]: '' }));

    // Real-time validatie voor startPrice
    if (name === 'startPrice') {
      let errorMsg = '';
      const startPriceNum = parseFloat(processedValue);
      if (!processedValue || isNaN(startPriceNum) || startPriceNum < 0) {
        errorMsg = 'Startprijs moet positief zijn';
      } else if (currentProduct && currentProduct.prijs !== null && currentProduct.prijs !== undefined) {
        const minPrice = currentProduct.prijs;
        if (startPriceNum < minPrice) {
          errorMsg = `Startprijs moet minimaal €${minPrice} zijn`;
        }
      }
      if (errorMsg) {
        setErrors(prev => ({ ...prev, startPrice: errorMsg }));
      }
    }
  };

  const validateForm = (): boolean => {
    const newErrors: FormErrors = { auctionDate:'', startPrice:'' };
    let isValid = true;

    if (!formData.auctionDate) { newErrors.auctionDate='Veilingdatum is verplicht'; isValid=false; }
    else if(new Date(formData.auctionDate) < new Date(new Date().toDateString())){ newErrors.auctionDate='Veilingdatum moet in de toekomst liggen'; isValid=false; }

    // Validatie voor startPrice
    const startPriceNum = parseFloat(formData.startPrice);
    if (!formData.startPrice || isNaN(startPriceNum) || startPriceNum < 0) {
      newErrors.startPrice = 'Startprijs moet positief zijn';
      isValid = false;
    } else if (currentProduct && currentProduct.prijs !== null && currentProduct.prijs !== undefined) {
      const minPrice = currentProduct.prijs;
      if (startPriceNum < minPrice) {
        newErrors.startPrice = `Startprijs moet minimaal €${minPrice} zijn`;
        isValid = false;
      }
    }

    setErrors(newErrors);
    return isValid;
  };

  useEffect(() => {
    const loadData = async () => {
    try{

      const response = await authFetch(`${API_BASE_URL}/api/Products/Status1`,
         { method: 'GET',
           headers: {
           'Content-Type': 'application/json',
            ...getAuthHeaders(),
          } });

      if(!response.ok){
        const error = await response.json();
        alert('Kon producten niet ophalen');
        console.error('API fout:');
        return;
      }
      const data: ProductDTO[] = await response.json();
      setProducts(data);
      console.log('Ontvangen producten:', data);

      } catch(err){
        console.error(err);
        alert('Er is een fout opgetreden bij het ophalen van producten');
      }
    }
      loadData();
    }, []);

    const handleSelectProduct = (product: ProductDTO) => {
    setCurrentProduct(product);
    setProducts(prev => {
      const others = prev.filter(p => p.id !== product.id);
      return [product, ...others];
    });
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    if (!currentProduct){
      alert('Selecteer eerst een product om in te plannen.');
      return;
    } else if (!currentProduct.id){
      alert('Invalide product geselecteerd.');
      return;
    }
    console.log('Submitting form data:');
    e.preventDefault();
    if(!validateForm()) return;

      if(!user || !user.KVK){
      alert("Kan leverancier niet bepalen. Log opnieuw in.");
      router.push('/login');
      return;
    }

    setIsSubmitting(true);

    try{
      // Normalize the price: replace comma with dot for decimal separation
      const normalizedPrice = formData.startPrice.replace(',', '.');
      const priceValue = parseFloat(normalizedPrice);
      
      if (isNaN(priceValue)) {
        alert('Ongeldig prijsformat');
        setIsSubmitting(false);
        return;
      }

      const submitData = new FormData();
      
      submitData.append('Id', currentProduct.id.toString());
      submitData.append('Datum', formData.auctionDate);
      submitData.append('StartPrijs', priceValue.toString());


      //MAAK ENDPOINT VOOR AAN
      const response = await authFetch(`${API_BASE_URL}/api/Products/productInplannen`, { method: 'POST', body: submitData });

      if(!response.ok){
        const error = await response.json();
        alert(`Registratie mislukt: ${error.message || 'Onbekende fout'}`);
        return;
      }

      alert('Product succesvol geregistreerd!');
      setFormData({auctionDate:'', startPrice:''});

    } catch(err){
      console.error(err);
      alert('Er is een fout opgetreden bij het inplannen van het product');
    } finally{
      setIsSubmitting(false);
    }
  };

  return (
    <div className="veilingMeesterProduct-page">
      <Topbar currentPage= "Product inplannen" />


      <div className="content">
        <form className="formContainerRight" onSubmit={handleSubmit}>

          <div className="inlineGroup">
            <div className="groupContainer">
              <label htmlFor="auctionDate">Veiling Datum:</label>
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

            <div className="groupContainer">
              <label htmlFor="startPrice">Start prijs (€):</label>
              <input
                id="startPrice"
                name="startPrice"
                type="text"
                placeholder={currentProduct?.prijs ? currentProduct.prijs.toString() : "0.00"}
                value={formData.startPrice}
                onChange={handleInputChange}
                required
              />
              {errors.startPrice && <div className="error-message">{errors.startPrice}</div>}
            </div>

          <div className="groupContainer">
            <input
              type="submit"
              className="submitButton"
              value={isSubmitting ? 'Registreren...' : 'Product inplannen'}
              disabled={isSubmitting}
            />
          </div>
        </form>

        {products.length > 0 && (
          <div className="productenContainer">
            {products.map((p) => (
              <div
                key={p.id}
                className={`producten ${currentProduct?.id === p.id ? 'selected' : ''}`}
                onClick={() => handleSelectProduct(p)}>
                <div> Product: {p.naam}</div>
                <div>{p.merk}</div>
                <div>Locatie: {p.locatie}</div>
                <div>Gewenste datum: {p.datum}</div>
                <div>Minimum prijs: €{p.prijs}</div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
