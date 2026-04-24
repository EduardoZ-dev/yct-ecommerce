import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ProductDto, CategoryDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {
  products = signal<ProductDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);

  formName = '';
  formDescription = '';
  formPrice = 0;
  formStock = 0;
  formImageUrl = '';
  formCategoryId = 0;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<ProductDto[]>>(`${environment.apiUrl}/api/Products`).subscribe({
      next: (res) => {
        this.products.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadCategories(): void {
    this.http.get<ResponseBase<CategoryDto[]>>(`${environment.apiUrl}/api/Categories`).subscribe({
      next: (res) => this.categories.set(res.data ?? [])
    });
  }

  openCreate(): void {
    this.formName = '';
    this.formDescription = '';
    this.formPrice = 0;
    this.formStock = 0;
    this.formImageUrl = '';
    this.formCategoryId = 0;
    this.editingId.set(null);
    this.showForm.set(true);
  }

  openEdit(p: ProductDto): void {
    this.formName = p.name;
    this.formDescription = p.description ?? '';
    this.formPrice = p.price;
    this.formStock = p.stock;
    this.formImageUrl = p.imageUrl ?? '';
    this.formCategoryId = p.categoryId;
    this.editingId.set(p.id);
    this.showForm.set(true);
  }

  save(): void {
    if (this.editingId()) {
      this.http.put<ResponseBase<ProductDto>>(`${environment.apiUrl}/api/Products/${this.editingId()}`, {
        id: this.editingId(),
        name: this.formName,
        description: this.formDescription,
        price: this.formPrice,
        stock: this.formStock,
        imageUrl: this.formImageUrl,
        isActive: true,
        categoryId: this.formCategoryId
      }).subscribe(() => {
        this.showForm.set(false);
        this.loadProducts();
      });
    } else {
      this.http.post<ResponseBase<ProductDto>>(`${environment.apiUrl}/api/Products`, {
        name: this.formName,
        description: this.formDescription,
        price: this.formPrice,
        stock: this.formStock,
        imageUrl: this.formImageUrl,
        categoryId: this.formCategoryId
      }).subscribe(() => {
        this.showForm.set(false);
        this.loadProducts();
      });
    }
  }

  delete(id: number): void {
    if (confirm('¿Eliminar este producto?')) {
      this.http.delete<ResponseBase<boolean>>(`${environment.apiUrl}/api/Products/${id}`).subscribe(() => {
        this.loadProducts();
      });
    }
  }

  cancel(): void {
    this.showForm.set(false);
  }
}
