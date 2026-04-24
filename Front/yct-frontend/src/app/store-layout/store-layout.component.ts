import { Component, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { CartService } from '../core/services/cart.service';
import { FooterComponent } from '../components/footer/footer.component';
import { WhatsappButtonComponent } from '../components/whatsapp-button/whatsapp-button.component';

@Component({
  selector: 'app-store-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, FooterComponent, WhatsappButtonComponent],
  templateUrl: './store-layout.component.html',
  styleUrl: './store-layout.component.scss'
})
export class StoreLayoutComponent {
  mobileMenuOpen = signal(false);
  userMenuOpen = signal(false);

  constructor(
    public auth: AuthService,
    public cart: CartService
  ) {
    // Escuchar cuando se agrega un item para hacer bounce en el carrito
    effect(() => {
      const count = this.cart.itemAdded();
      if (count > 0) {
        this.bounceCartIcon();
      }
    });
  }

  private bounceCartIcon(): void {
    try {
      const cartEl = document.querySelector('.cart-icon');
      if (!cartEl) return;
      cartEl.classList.remove('cart-bounce');
      // Force reflow para reiniciar la animación
      void (cartEl as HTMLElement).offsetWidth;
      cartEl.classList.add('cart-bounce');
      setTimeout(() => cartEl.classList.remove('cart-bounce'), 600);
    } catch {
      // No romper nada si falla
    }
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  toggleUserMenu(): void {
    this.userMenuOpen.update(v => !v);
  }

  logout(): void {
    this.auth.logout();
    this.userMenuOpen.set(false);
  }
}
