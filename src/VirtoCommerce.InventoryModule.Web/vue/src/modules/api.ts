import { useApiClient } from "@vc-shell/framework";
import { InventoryModuleClient } from "../api_client/virtocommerce.inventory";

export function useInventoryClient() {
  return useApiClient(InventoryModuleClient).getApiClient;
}

// Other modules' endpoints this UI reads (countries, products): plain same-origin fetches
export async function http<T>(url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method: body === undefined ? "GET" : "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}: ${url}`);
  const text = await res.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export interface ProductEntry {
  id: string;
  name: string;
  code?: string;
  imageUrl?: string;
}

export async function searchProducts(keyword: string | undefined, skip: number, take: number) {
  const r = await http<{ listEntries?: ProductEntry[]; totalCount?: number }>("/api/catalog/listentries", {
    keyword,
    objectTypes: ["CatalogProduct"],
    searchInChildren: true,
    searchInVariations: true,
    skip,
    take,
  });
  return { results: r.listEntries ?? [], totalCount: r.totalCount ?? 0 };
}
