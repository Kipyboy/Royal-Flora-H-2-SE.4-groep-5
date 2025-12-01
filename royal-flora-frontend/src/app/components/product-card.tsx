import React from "react";
import '../../styles/ProductCard.css';



interface ProductCardProps {
    naam: string;
    merk: string;
    prijs: string;
    datum: string;
    locatie: string;
    status: string;
    aantal: number;
    fotoPath: string;
}

const ProductCard: React.FC<ProductCardProps> = ({
    naam,
    merk,
    prijs,
    datum,
    locatie,
    status,
    aantal,
    fotoPath
}) => {
    const defaultImg = "https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg";
    return (
    <div className="product-card">
        <div className="image">
            <img src={fotoPath && fotoPath.trim() !== "" ? `http://localhost:5156/images/${fotoPath}` : defaultImg} alt="" id="product-foto"/>
        </div>
        <div className="info">
            <p id="naam">Naam: {naam}</p>
            <p id="merk">Merk: {merk}</p>
            <p id="aantal">Aantal: {aantal}</p>
            <p id="prijs">Prijs: {prijs}</p>
            <p id="datum">Datum: {datum}</p>
            <p id="locatie">Locatie: {locatie}</p>
            <p id="status">Status: {status}</p>
        </div>
    </div>
    );
};

export default ProductCard;