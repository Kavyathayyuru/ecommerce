import { useContext } from "react";
import { CartContext } from "./CartContextDefinition";

export function useCart() {
    return useContext(CartContext);
}