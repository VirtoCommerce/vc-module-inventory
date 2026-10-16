import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { markRaw } from "vue";
import * as blades from "./pages";
import * as locales from "./locales";
import ProductInventoryWidget from "./components/ProductInventoryWidget.vue";
import StoreCentersWidget from "./components/StoreCentersWidget.vue";
// registers the dashboard widgets (must run before the host builds the dashboard)
import "./dashboard";

// Widgets the legacy module registered into other modules' blades ("itemDetail", "storeDetail")
registerExternalWidget({
  id: "InventoryProductStock",
  title: "Inventory",
  component: markRaw(ProductInventoryWidget),
  targetBlades: ["CatalogProductDetails"],
  // nothing to show for an unsaved product/store
  isVisible: (blade) => !!blade?.param,
});
registerExternalWidget({
  id: "InventoryStoreFulfillmentCenters",
  title: "Fulfillment centers",
  component: markRaw(StoreCentersWidget),
  targetBlades: ["StoreDetails"],
  // nothing to show for an unsaved product/store
  isVisible: (blade) => !!blade?.param,
});

export default defineAppModule({
  blades,
  locales,
});
