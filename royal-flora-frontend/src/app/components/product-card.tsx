import React from "react";
import '../../styles/ProductCard.css';

const ProductCard: React.FC = () => 

    <div className="product-card">
        <div className="image">
            <img src="https://syria.adra.cloud/wp-content/uploads/2021/10/empty.jpg" alt="" />
        </div>
        <div className="info">
            <p id="naam">Naam:</p>
            <p id="merk">Merk:</p>
            <p id="prijs">Prijs:</p>
            <p id="datum">Datum:</p>
            <p id="locatie">Locatie:</p>
            <p id="opbrengst/gekocht">Opbrengst:</p>
        </div>
    </div>


export default ProductCard;