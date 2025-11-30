import React from "react";
import '../../styles/ProductCard.css';


interface ProductCardProps {
    naam: string;
    merk: string;
    prijs: string | number;
    datum: string;
    locatie: string;
    status: string;
    Aantal: number;
}

const formatPrice = (price: string | number): string => {
    const numPrice = typeof price === 'string' ? parseFloat(price) : price;
    if (isNaN(numPrice)) {
        return "€0.00";
    }
    return new Intl.NumberFormat('nl-NL', {
        style: 'currency',
        currency: 'EUR'
    }).format(numPrice);
};

const ProductCard: React.FC<ProductCardProps> = ({
    naam,
    merk,
    prijs,
    datum,
    locatie,
    status,
    Aantal
}) => 

    <div className="product-card">
        <div className="image">
            <img src="https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg" alt="" />
        </div>
        <div className="info">
            <p id="naam">Naam: {naam}</p>
            <p id="merk">Merk: {merk}</p>
            <p id="aantal">Aantal: {Aantal}</p>
            <p id="prijs">Prijs: {formatPrice(prijs)}</p>
            <p id="datum">Datum: {datum}</p>
            <p id="locatie">Locatie: {locatie}</p>
            <p id="status">Status: {status}</p>
        </div>
    </div>


export default ProductCard;