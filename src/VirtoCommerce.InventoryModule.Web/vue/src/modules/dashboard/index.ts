// Inventory widgets on the Admin Shell dashboard. Every widget declares a position (the grid only honours declared
// positions when all widgets have one); the rows follow the design's Dashboard.jsx: low stock sits at y16, left of
// Orders' "Orders to process".
import { registerDashboardWidget } from "@vc-shell/framework";
import { markRaw } from "vue";
import LowStockWidget from "./LowStockWidget.vue";

registerDashboardWidget({
  id: "inventory-low-stock",
  name: "Low stock",
  component: markRaw(LowStockWidget),
  size: { width: 8, height: 6 },
  position: { x: 0, y: 16 },
  permissions: ["inventory:read"],
});
