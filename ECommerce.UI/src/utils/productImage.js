const API_ORIGIN = "https://localhost:7070";

const imageCatalog = [
    { keywords: ["laptop", "notebook", "thinkpad", "dell", "hp", "lenovo", "asus", "acer"], images: [
        "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["iphone", "ipad"], images: [
        "https://images.unsplash.com/photo-1592286927505-2fd0a6f7a2af?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["samsung", "galaxy", "oneplus", "pixel", "mobile", "phone"], images: [
        "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1556656793-08538906a9f8?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["airpods", "earbuds", "headphone", "headset", "sony", "boat"], images: [
        "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1572569511254-d8f925fe2cbb?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["speaker", "soundbar"], images: [
        "https://images.unsplash.com/photo-1545454675-3531b543be5d?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["keyboard"], images: [
        "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1595225476474-87563907a212?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["mouse"], images: [
        "https://images.unsplash.com/photo-1527814050087-3793815479db?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["camera", "canon", "nikon"], images: [
        "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1606986628253-5b27c5d5f9b6?auto=format&fit=crop&w=700&q=85"
    ] },
    { keywords: ["watch"], images: [
        "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=700&q=85",
        "https://images.unsplash.com/photo-1547996160-81dfa63595aa?auto=format&fit=crop&w=700&q=85"
    ] }
];

const defaultImage = "https://images.unsplash.com/photo-1556228578-8c89e6adf883?auto=format&fit=crop&w=700&q=85";
const genericImages = [
    "https://images.unsplash.com/photo-1498049794561-7780e7231661?auto=format&fit=crop&w=700&q=85",
    "https://images.unsplash.com/photo-1468495244123-6c6c332eeece?auto=format&fit=crop&w=700&q=85",
    "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=700&q=85",
    "https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=700&q=85"
];

function stableIndex(value, length) {
    const hash = [...String(value || "")].reduce((total, character) => total + character.charCodeAt(0), 0);
    return hash % length;
}

export function getProductImage(product) {
    const suppliedImage = product?.imageUrl || product?.imageURL || product?.image ||
        product?.imagePath || product?.photoUrl || product?.picture;

    if (suppliedImage) {
        if (/^https?:\/\//i.test(suppliedImage)) return suppliedImage;
        return `${API_ORIGIN}/${String(suppliedImage).replace(/^\/+/, "")}`;
    }

    const productText = `${product?.name || ""} ${product?.description || ""}`.toLowerCase();
    const match = imageCatalog.find((entry) =>
        entry.keywords.some((keyword) => productText.includes(keyword))
    );

    if (match) {
        return match.images[stableIndex(product?.id || product?.name, match.images.length)];
    }

    return genericImages[stableIndex(product?.id || product?.name, genericImages.length)];
}

export const imageFallback = defaultImage;
