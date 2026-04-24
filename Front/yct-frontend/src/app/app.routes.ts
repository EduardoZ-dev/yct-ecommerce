import { Routes } from '@angular/router';
import { StoreLayoutComponent } from './store-layout/store-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: StoreLayoutComponent,
    children: [
      { path: 'shop', loadComponent: () => import('./features/shop/shop.component').then(m => m.ShopComponent) },
      { path: 'catalog', loadComponent: () => import('./features/catalog/catalog.component').then(m => m.CatalogComponent) },
      { path: 'product/:id', loadComponent: () => import('./features/product-detail/product-detail.component').then(m => m.ProductDetailComponent) },
      {
        path: 'payment-info',
        loadComponent: () => import('./features/info-page/info-page.component').then(m => m.InfoPageComponent),
        data: { type: 'payment' }
      },
      {
        path: 'shipping-info',
        loadComponent: () => import('./features/info-page/info-page.component').then(m => m.InfoPageComponent),
        data: { type: 'shipping' }
      },
      {
        path: 'quality',
        loadComponent: () => import('./features/info-page/info-page.component').then(m => m.InfoPageComponent),
        data: { type: 'quality' }
      },
      {
        path: 'contact',
        loadComponent: () => import('./features/info-page/info-page.component').then(m => m.InfoPageComponent),
        data: { type: 'contact' }
      },
      { path: 'about', loadComponent: () => import('./features/about/about.component').then(m => m.AboutComponent) },
      { path: 'cart', loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent) },
      { path: 'checkout', loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent) },
      { path: 'orders', loadComponent: () => import('./features/orders/orders.component').then(m => m.OrdersComponent) },
      { path: '', redirectTo: 'shop', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'shop' }
];
