import React, { useState } from "react";
import '../../styles/ProductCard.css';
import { API_BASE_URL } from '../config/api';



interface ProductCardProps {
    naam: string;
    merk: string;
    prijs: string | number;
    datum: string;
    locatie: string;
    status: string;
    aantal: number;
    fotoPath: string;
    beschrijving: string;
    toonBeschrijving: boolean;
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
    aantal,
    fotoPath
    , beschrijving,
    toonBeschrijving
}) => {
    
    const defaultImg = "https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg";
    return (
    <div className="product-card">
        <div className="image">
            <img src={fotoPath && fotoPath.trim() !== "" ? `${API_BASE_URL}/images/${fotoPath}` : defaultImg} alt="" id="product-foto"/>
        </div>
        <div className="info" style={{ display : toonBeschrijving ? 'none' : 'block'}}>
            <p id="naam">Naam: {naam}</p>
            <p id="merk">Merk: {merk}</p>
            <p id="aantal">Aantal: {aantal}</p>
            <p id="prijs">Prijs: {prijs}</p>
            <p id="datum">Datum: {datum}</p>
            <p id="locatie">Locatie: {locatie}</p>
            <p id="status">Status: {status}</p>
        </div>
        <div className="beschrijving" style={{ display: toonBeschrijving ? 'block' : 'none' }}>
            {beschrijving}
        </div>
    </div>
    );
};

export default ProductCard;