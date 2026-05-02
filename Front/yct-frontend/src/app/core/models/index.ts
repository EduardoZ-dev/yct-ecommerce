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
  phone?: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

// Audit log
export interface AuditLogDto {
  id: number;
  userId: number | null;
  username: string;
  userRole: string;
  action: string;
  entityType: string;
  entityId: number | null;
  summary: string;
  details: string | null;
  ipAddress: string | null;
  success: boolean;
  createdAt: string;
}

export interface AuditLogPageDto {
  items: AuditLogDto[];
  total: number;
  page: number;
  pageSize: number;
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
export type PaymentMethod = 'OnDelivery' | 'Transfer' | 'Cash';
export type PaymentStatus = 'Unpaid' | 'Paid' | 'Refunded';

export interface OrderDto {
  id: number;
  orderNumber: string;
  consecutive: number;
  orderDate: string;
  total: number;
  status: string;
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
  paidAt?: string | null;
  notes?: string;
  shippingAddress?: string;
  shippingCity?: string;
  shippingLat?: number;
  shippingLng?: number;
  validatedAt?: string | null;
  shippedAt?: string | null;
  deliveredAt?: string | null;
  distributorId?: number | null;
  distributorName?: string | null;
  distributorVehicle?: string | null;
  distributorPhone?: string | null;
  trackingNumber?: string | null;
  customerRating?: number | null;
  feedbackComment?: string | null;
  userId: number;
  userFullName: string;
  userPhone?: string;
  details: OrderDetailDto[];
}

export interface DistributorDto {
  id: number;
  name: string;
  phone?: string | null;
  vehicleType: string;
  vehiclePlate?: string | null;
  notes?: string | null;
  isActive: boolean;
  createdAt: string;
  ordersCount: number;
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
