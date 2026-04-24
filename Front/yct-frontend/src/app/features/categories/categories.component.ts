import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CategoryDto, ResponseBase } from '../../core/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit {
  categories = signal<CategoryDto[]>([]);
  loading = signal(false);
  showForm = signal(false);
  editingId = signal<number | null>(null);

  formName = '';
  formDescription = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.http.get<ResponseBase<CategoryDto[]>>(`${environment.apiUrl}/api/Categories`).subscribe({
      next: (res) => {
        this.categories.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.formName = '';
    this.formDescription = '';
    this.editingId.set(null);
    this.showForm.set(true);
  }

  openEdit(cat: CategoryDto): void {
    this.formName = cat.name;
    this.formDescription = cat.description ?? '';
    this.editingId.set(cat.id);
    this.showForm.set(true);
  }

  save(): void {
    if (this.editingId()) {
      this.http.put<ResponseBase<CategoryDto>>(`${environment.apiUrl}/api/Categories/${this.editingId()}`, {
        id: this.editingId(),
        name: this.formName,
        description: this.formDescription,
        isActive: true
      }).subscribe(() => {
        this.showForm.set(false);
        this.loadCategories();
      });
    } else {
      this.http.post<ResponseBase<CategoryDto>>(`${environment.apiUrl}/api/Categories`, {
        name: this.formName,
        description: this.formDescription
      }).subscribe(() => {
        this.showForm.set(false);
        this.loadCategories();
      });
    }
  }

  delete(id: number): void {
    if (confirm('¿Eliminar esta categoría?')) {
      this.http.delete<ResponseBase<boolean>>(`${environment.apiUrl}/api/Categories/${id}`).subscribe(() => {
        this.loadCategories();
      });
    }
  }

  cancel(): void {
    this.showForm.set(false);
  }
}
