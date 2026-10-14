<template>
  <VcBlade
    :title="title"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="60%"
  >
    <VcContainer>
      <VcForm class="inventory-form tw-space-y-4">
        <VcInput
          v-model="item.name"
          :label="$t('INVENTORY.CENTERS.FIELDS.NAME')"
          :hint="$t('INVENTORY.CENTERS.FIELDS.NAME_HINT')"
          required
        />
        <VcRow class="tw-gap-4">
          <VcCol>
            <VcInput
              v-model="item.geoLocation"
              :label="$t('INVENTORY.CENTERS.FIELDS.GEO')"
              :hint="geoValid ? $t('INVENTORY.CENTERS.FIELDS.GEO_HINT') : undefined"
              :error="!geoValid"
              :error-message="geoValid ? undefined : $t('INVENTORY.CENTERS.FIELDS.GEO_INVALID')"
            />
          </VcCol>
          <VcCol>
            <VcInput
              v-model="item.outerId"
              :label="$t('INVENTORY.CENTERS.FIELDS.OUTER_ID')"
              :hint="$t('INVENTORY.CENTERS.FIELDS.OUTER_ID_HINT')"
            />
          </VcCol>
        </VcRow>

        <!-- legacy: the Address widget opening the core address blade -->
        <VcCard :header="$t('INVENTORY.CENTERS.ADDRESS.TITLE')">
          <div class="tw-p-4 tw-space-y-4">
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcSelect
                  :model-value="address.countryCode"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.COUNTRY')"
                  :options="countries"
                  option-value="id"
                  option-label="name"
                  searchable
                  @update:model-value="setCountry"
                />
              </VcCol>
              <VcCol>
                <VcSelect
                  v-if="regions.length"
                  :model-value="address.regionId"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.REGION')"
                  :placeholder="address.regionName"
                  :options="regions"
                  option-value="id"
                  option-label="name"
                  searchable
                  @update:model-value="setRegion"
                />
                <VcInput
                  v-else
                  v-model="address.regionName"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.REGION')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="address.city"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.CITY')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="address.postalCode"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.POSTAL_CODE')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="address.line1"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.LINE1')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="address.line2"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.LINE2')"
                />
              </VcCol>
            </VcRow>
            <VcRow class="tw-gap-4">
              <VcCol>
                <VcInput
                  v-model="address.email"
                  type="email"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.EMAIL')"
                />
              </VcCol>
              <VcCol>
                <VcInput
                  v-model="address.phone"
                  :label="$t('INVENTORY.CENTERS.ADDRESS.PHONE')"
                />
              </VcCol>
            </VcRow>
          </div>
        </VcCard>

        <VcTextarea
          v-model="item.shortDescription"
          :label="$t('INVENTORY.CENTERS.FIELDS.SHORT_DESCRIPTION')"
        />
        <VcEditor
          v-model="item.description"
          :label="$t('INVENTORY.CENTERS.FIELDS.DESCRIPTION')"
          assets-folder="cms-content/image/assets"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useAsync, useBlade, useBladeForm, useBladeWidgets, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBlade,
  VcCard,
  VcCol,
  VcContainer,
  VcEditor,
  VcForm,
  VcInput,
  VcRow,
  VcSelect,
  VcTextarea,
} from "@vc-shell/framework/ui";
import { computed, nextTick, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { http, useInventoryClient } from "../api";
import type { FulfillmentCenter, InventoryAddress } from "../../api_client/virtocommerce.inventory";

defineBlade({ url: "/fulfillment-center", name: "InventoryFulfillmentCenterDetails", permissions: ["inventory:fulfillment:read"] });

type Named = { id: string; name: string };

const { t } = useI18n({ useScope: "global" });
const { param, openBlade, callParent, closeSelf, replaceWith } = useBlade();
const { showConfirmation } = usePopup();
const client = useInventoryClient();

const item = ref<FulfillmentCenter>({});
const countries = ref<Named[]>([]);
const regions = ref<Named[]>([]);
const productsCount = ref<number | string>("…");
const isNew = computed(() => !param.value);
const title = computed(() => (isNew.value ? t("INVENTORY.CENTERS.NEW") : (item.value.name ?? "")));
const address = computed(() => (item.value.address ??= {}) as InventoryAddress);
const geoValid = computed(() => !item.value.geoLocation || /^(-?\d+(\.\d+)?),\s*(-?\d+(\.\d+)?)$/.test(item.value.geoLocation));

const form = useBladeForm({
  data: item,
  canSaveOverride: computed(() => !!item.value.name?.trim() && geoValid.value),
  closeConfirmMessage: computed(() => t("INVENTORY.ALERTS.CLOSE_CONFIRMATION")),
});

// The HTML editor rewrites the loaded markup on mount (drops attributes it doesn't support). The baseline is
// taken after that, and an unedited description is saved as it was loaded, not in the rewritten form.
let loadedDescription: { original?: string; rewritten?: string } = {};
async function baseline() {
  const original = item.value.description;
  await nextTick();
  await new Promise((r) => setTimeout(r, 0));
  loadedDescription = { original, rewritten: item.value.description };
  form.setBaseline();
}

async function loadRegions(countryCode?: string) {
  regions.value = countryCode ? ((await http<Named[]>(`/api/platform/common/countries/${countryCode}/regions`)) ?? []) : [];
}

function setCountry(id: unknown) {
  const country = countries.value.find((c) => c.id === id);
  Object.assign(address.value, { countryCode: country?.id, countryName: country?.name, regionId: undefined, regionName: undefined });
  loadRegions(country?.id);
}

function setRegion(id: unknown) {
  const region = regions.value.find((r) => r.id === id);
  Object.assign(address.value, { regionId: region?.id, regionName: region?.name });
}

async function refreshCount() {
  if (isNew.value) return;
  const r = await (await client()).searchProductInventoryInfos({ fulfillmentCenterIds: [param.value!], withPositiveQuantityOnly: true, take: 0 });
  productsCount.value = r.totalCount ?? 0;
}

const { loading, action: load } = useAsync(async () => {
  item.value = await (await client()).getFulfillmentCenter(param.value!);
  await loadRegions(item.value.address?.countryCode);
  await baseline();
});

const { loading: saving, action: save } = useAsync(async () => {
  const body = { ...item.value };
  if (body.description === loadedDescription.rewritten) body.description = loadedDescription.original;
  const saved = await (await client()).saveFulfillmentCenter(body);
  form.setBaseline();
  callParent("reload");
  if (isNew.value) {
    await replaceWith({ name: "InventoryFulfillmentCenterDetails", param: saved.id });
    return;
  }
  await load();
});

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "save",
    title: computed(() => t(isNew.value ? "INVENTORY.TOOLBAR.CREATE" : "INVENTORY.TOOLBAR.SAVE")),
    icon: "lucide-save",
    permissions: "inventory:fulfillment:edit",
    disabled: computed(() => !form.canSave.value || saving.value),
    clickHandler: () => save(),
  },
  {
    id: "reset",
    title: t("INVENTORY.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    isVisible: computed(() => !isNew.value),
    disabled: computed(() => !form.isModified.value),
    clickHandler: () => form.revert(),
  },
  {
    id: "delete",
    title: t("INVENTORY.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "inventory:fulfillment:delete",
    isVisible: computed(() => !isNew.value),
    async clickHandler() {
      if (!(await showConfirmation(t("INVENTORY.ALERTS.DELETE_CENTER", { name: item.value.name })))) return;
      await (await client()).deleteFulfillmentCenters([param.value!]);
      form.setBaseline();
      callParent("reload");
      await closeSelf();
    },
  },
]);

useBladeWidgets([
  {
    id: "InventoryCenterProducts",
    icon: "lucide-package",
    title: "INVENTORY.CENTERS.WIDGETS.PRODUCTS",
    badge: productsCount,
    isVisible: computed(() => !isNew.value),
    onClick: () =>
      openBlade({
        name: "InventoryFulfillmentCenterProducts",
        param: param.value,
        options: { name: item.value.name },
        onClose: refreshCount,
      }),
    onRefresh: refreshCount,
  },
]);

// the country list, the center and its products count load side by side
onMounted(async () => {
  const countriesLoaded = http<Named[]>("/api/platform/common/countries").then((x) => (countries.value = x));
  if (isNew.value) {
    item.value = { address: {} } as FulfillmentCenter;
    await baseline();
  } else {
    refreshCount();
    await load();
  }
  await countriesLoaded;
});
</script>

<style lang="scss">
/* the design's blade body padding, as in the Orders details */
.inventory-form {
  padding: 20px 22px 36px;
}
</style>
