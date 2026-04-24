// Auth
export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
  fullName: string;
  email?: string;
  phone?: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  fullName: string;
  role: string;
}

export interface UserDto {
  id: number;
  username: string;
  fullName: string;
  email?: string;
  role: string;
}

// Response
export interface ResponseBase<T> {
  success: boolean;
  message: string;
  data: T;
}

// Category
export interface CategoryDto {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  productCount: number;
}

// Product
export interface ProductDto {
  id: number;
  name: string;
  description?: string;
  price: number;
  stock: number;
  imageUrl?: string;
  isActive: boolean;
  categoryId: number;
  categoryName: string;
  // Info producto
  weight?: string;
  ingredients?: string;
  storageInstructions?: string;
  expirationInfo?: string;
  brand?: string;
  // Nutricional
  servingSize?: string;
  calories?: number;
  totalFat?: number;
  saturatedFat?: number;
  cholesterol?: number;
  sodium?: number;
  totalCarbs?: number;
  sugars?: number;
  protein?: number;
  calcium?: number;
  iron?: number;
  vitaminD?: number;
}

// Order
export interface OrderDto {
  id: number;
  orderNumber: string;
  orderDate: string;
  total: number;
  status: string;
  notes?: string;
  shippingAddress?: string;
  userId: number;
  userFullName: string;
  details: OrderDetailDto[];
}

export interface OrderDetailDto {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface CreateOrderRequest {
  userId: number;
  notes?: string;
  shippingAddress?: string;
  details: { productId: number; quantity: number }[];
}
