import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { ProductDto, CategoryDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';
import { IconComponent } from '../../components/icon/icon.component';

interface ProductForm {
  name: string;
  description: string;
  price: number | null;
  stock: number | null;
  imageUrl: string;
  isActive: boolean;
  categoryId: number;
  brand: string;
  weight: string;
  ingredients: string;
  storageInstructions: string;
  expirationInfo: string;
  servingSize: string;
  calories: number | null;
  totalFat: number | null;
  saturatedFat: number | null;
  cholesterol: number | null;
  sodium: number | null;
  totalCarbs: number | null;
  sugars: number | null;
  protein: number | null;
  calcium: number | null;
  iron: number | null;
  vitaminD: number | null;
}

const emptyForm = (): ProductForm => ({
  name: '', description: '', price: null, stock: null, imageUrl: '',
  isActive: true, categoryId: 0,
  brand: '', weight: '', ingredients: '', storageInstructions: '', expirationInfo: '',
  servingSize: '', calories: null, totalFat: null, saturatedFat: null,
  cholesterol: null, sodium: null, totalCarbs: null, sugars: null,
  protein: null, calcium: null, iron: null, vitaminD: null
});

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule, IconComponent],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {
  private http = inject(HttpClient);

  products = signal<ProductDto[]>([]);
  categories = signal<CategoryDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);
  formError = signal('');
  formSection = signal<'basic' | 'info' | 'nutrition'>('basic');

  // Filtros
  searchTerm = signal('');
  filterCategory = signal<number | null>(null);
  filterStatus = signal<'all' | 'active' | 'inactive'>('all');

  form = signal<ProductForm>(emptyForm());

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    const cat = this.filterCategory();
    const status = this.filterStatus();
    return this.products().filter(p => {
      if (cat !== null && p.categoryId !== cat) return false;
      if (status === 'active' && !p.isActive) return false;
      if (status === 'inactive' && p.isActive) return false;
      if (term) {
        const haystack = `${p.name} ${p.brand ?? ''} ${p.categoryName}`.toLowerCase();
        if (!haystack.includes(term)) return false;
      }
      return true;
    });
  });

  // Conteo de productos por categoría (respeta search + status, no la cat seleccionada)
  categoryCounts = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    const status = this.filterStatus();
    const map = new Map<number, number>();
    let total = 0;

    for (const p of this.products()) {
      if (status === 'active' && !p.isActive) continue;
      if (status === 'inactive' && p.isActive) continue;
      if (term) {
        const haystack = `${p.name} ${p.brand ?? ''} ${p.categoryName}`.toLowerCase();
        if (!haystack.includes(term)) continue;
      }
      total++;
      map.set(p.categoryId, (map.get(p.categoryId) ?? 0) + 1);
    }
    return { map, total };
  });

  categoryCount(id: number): number {
    return this.categoryCounts().map.get(id) ?? 0;
  }

  // Drag-drop sólo se permite cuando no hay filtros aplicados
  canReorder = computed(() =>
    !this.searchTerm().trim() &&
    this.filterCategory() === null &&
    this.filterStatus() === 'all'
  );

  onDrop(event: CdkDragDrop<ProductDto[]>): void {
    if (!this.canReorder()) return;
    if (event.previousIndex === event.currentIndex) return;

    const list = [...this.products()];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    this.products.set(list);

    // Mandar el nuevo orden al backend (índice 1-based)
    const payload = {
      items: list.map((p, idx) => ({ id: p.id, displayOrder: idx + 1 }))
    };
    this.http.post<ResponseBase<boolean>>(
      `${environment.apiUrl}/api/Products/reorder`, payload
    ).subscribe({
      error: () => this.loadProducts() // si falla, recargar para volver al estado real
    });
  }

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

  setFilterCategory(id: number | null): void { this.filterCategory.set(id); }
  setFilterStatus(s: 'all' | 'active' | 'inactive'): void { this.filterStatus.set(s); }
  clearFilters(): void {
    this.searchTerm.set('');
    this.filterCategory.set(null);
    this.filterStatus.set('all');
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.set(emptyForm());
    this.formSection.set('basic');
    this.formError.set('');
    this.showForm.set(true);
    document.body.style.overflow = 'hidden';
  }

  openEdit(p: ProductDto): void {
    this.editingId.set(p.id);
    this.form.set({
      name: p.name,
      description: p.description ?? '',
      price: p.price,
      stock: p.stock,
      imageUrl: p.imageUrl ?? '',
      isActive: p.isActive,
      categoryId: p.categoryId,
      brand: p.brand ?? '',
      weight: p.weight ?? '',
      ingredients: p.ingredients ?? '',
      storageInstructions: p.storageInstructions ?? '',
      expirationInfo: p.expirationInfo ?? '',
      servingSize: p.servingSize ?? '',
      calories: p.calories ?? null,
      totalFat: p.totalFat ?? null,
      saturatedFat: p.saturatedFat ?? null,
      cholesterol: p.cholesterol ?? null,
      sodium: p.sodium ?? null,
      totalCarbs: p.totalCarbs ?? null,
      sugars: p.sugars ?? null,
      protein: p.protein ?? null,
      calcium: p.calcium ?? null,
      iron: p.iron ?? null,
      vitaminD: p.vitaminD ?? null
    });
    this.formSection.set('basic');
    this.formError.set('');
    this.showForm.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeForm(): void {
    this.showForm.set(false);
    this.formError.set('');
    document.body.style.overflow = '';
  }

  save(): void {
    const f = this.form();
    if (!f.name.trim()) { this.formError.set('El nombre es obligatorio'); return; }
    if (f.price === null || f.price < 0) { this.formError.set('El precio es obligatorio'); return; }
    if (f.stock === null || f.stock < 0) { this.formError.set('El stock es obligatorio'); return; }
    if (!f.categoryId) { this.formError.set('Debes seleccionar una categoría'); return; }

    const payload: any = {
      name: f.name.trim(),
      description: f.description.trim() || null,
      price: f.price,
      stock: f.stock,
      imageUrl: f.imageUrl.trim() || null,
      categoryId: f.categoryId,
      brand: f.brand.trim() || null,
      weight: f.weight.trim() || null,
      ingredients: f.ingredients.trim() || null,
      storageInstructions: f.storageInstructions.trim() || null,
      expirationInfo: f.expirationInfo.trim() || null,
      servingSize: f.servingSize.trim() || null,
      calories: f.calories,
      totalFat: f.totalFat,
      saturatedFat: f.saturatedFat,
      cholesterol: f.cholesterol,
      sodium: f.sodium,
      totalCarbs: f.totalCarbs,
      sugars: f.sugars,
      protein: f.protein,
      calcium: f.calcium,
      iron: f.iron,
      vitaminD: f.vitaminD
    };

    const id = this.editingId();
    const req = id
      ? this.http.put<ResponseBase<ProductDto>>(
          `${environment.apiUrl}/api/Products/${id}`,
          { id, isActive: f.isActive, ...payload }
        )
      : this.http.post<ResponseBase<ProductDto>>(
          `${environment.apiUrl}/api/Products`, payload
        );

    req.subscribe({
      next: (res) => {
        if (res.success) {
          this.closeForm();
          this.loadProducts();
        } else {
          this.formError.set(res.message);
        }
      },
      error: (err) => this.formError.set(err.error?.message ?? 'Error al guardar')
    });
  }

  delete(p: ProductDto): void {
    if (!confirm(`¿Eliminar "${p.name}"?\nEsta acción no se puede deshacer.`)) return;
    this.http.delete<ResponseBase<boolean>>(`${environment.apiUrl}/api/Products/${p.id}`).subscribe({
      next: () => this.loadProducts()
    });
  }

  toggleActive(p: ProductDto): void {
    const updated = { ...p, isActive: !p.isActive };
    this.http.put<ResponseBase<ProductDto>>(
      `${environment.apiUrl}/api/Products/${p.id}`,
      {
        id: p.id, name: p.name, description: p.description, price: p.price,
        stock: p.stock, imageUrl: p.imageUrl, categoryId: p.categoryId,
        isActive: updated.isActive,
        brand: p.brand, weight: p.weight, ingredients: p.ingredients,
        storageInstructions: p.storageInstructions, expirationInfo: p.expirationInfo,
        servingSize: p.servingSize, calories: p.calories, totalFat: p.totalFat,
        saturatedFat: p.saturatedFat, cholesterol: p.cholesterol, sodium: p.sodium,
        totalCarbs: p.totalCarbs, sugars: p.sugars, protein: p.protein,
        calcium: p.calcium, iron: p.iron, vitaminD: p.vitaminD
      }
    ).subscribe({
      next: () => this.loadProducts()
    });
  }

  updateForm<K extends keyof ProductForm>(key: K, value: ProductForm[K]): void {
    this.form.update(f => ({ ...f, [key]: value }));
  }

  uploadingImage = signal(false);

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.formError.set('El archivo debe ser una imagen');
      input.value = '';
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.formError.set('La imagen no puede superar 2 MB');
      input.value = '';
      return;
    }

    const fd = new FormData();
    fd.append('file', file);
    this.uploadingImage.set(true);
    this.formError.set('');

    this.http.post<ResponseBase<string>>(
      `${environment.apiUrl}/api/Upload/image`, fd
    ).subscribe({
      next: (res) => {
        this.uploadingImage.set(false);
        if (res.success && res.data) {
          this.updateForm('imageUrl', res.data);
        } else {
          this.formError.set(res.message ?? 'Error al subir la imagen');
        }
        input.value = '';
      },
      error: (err) => {
        this.uploadingImage.set(false);
        this.formError.set(err.error?.message ?? 'No se pudo subir la imagen');
        input.value = '';
      }
    });
  }

  clearImage(): void {
    this.updateForm('imageUrl', '');
  }

  stockClass(stock: number): string {
    if (stock === 0) return 'out';
    if (stock <= 20) return 'low';
    return 'ok';
  }
}
