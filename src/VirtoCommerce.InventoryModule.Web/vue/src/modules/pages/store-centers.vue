<template>
  <!-- The store's fulfillment centers (legacy storeFulfillmentWidget): edits the store of the parent Store blade,
       which saves them with the store -->
  <VcBlade
    :title="$t('INVENTORY.STORE.TITLE', { name: store?.name ?? '' })"
    :toolbar-items="bladeToolbar"
    width="50%"
  >
    <VcContainer>
      <VcForm class="inventory-form tw-space-y-4">
        <VcSelect
          v-model="edited.mainFulfillmentCenterId"
          :label="$t('INVENTORY.STORE.MAIN')"
          :placeholder="$t('INVENTORY.STORE.PLACEHOLDER')"
          :options="searchCenters"
          option-value="id"
          option-label="name"
          searchable
        />
        <VcSelect
          v-model="edited.additionalFulfillmentCenterIds"
          :label="$t('INVENTORY.STORE.ADDITIONAL')"
          :placeholder="$t('INVENTORY.STORE.PLACEHOLDER')"
          :options="searchCenters"
          option-value="id"
          option-label="name"
          multiple
          searchable
        />
        <VcSelect
          v-model="edited.mainReturnsFulfillmentCenterId"
          :label="$t('INVENTORY.STORE.RETURNS')"
          :placeholder="$t('INVENTORY.STORE.PLACEHOLDER')"
          :options="searchCenters"
          option-value="id"
          option-label="name"
          searchable
        />
        <VcSelect
          v-model="edited.returnsFulfillmentCenterIds"
          :label="$t('INVENTORY.STORE.ADDITIONAL_RETURNS')"
          :placeholder="$t('INVENTORY.STORE.PLACEHOLDER')"
          :options="searchCenters"
          option-value="id"
          option-label="name"
          multiple
          searchable
        />
        <VcHint>{{ $t("INVENTORY.STORE.HINT") }}</VcHint>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcHint, VcSelect } from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useInventoryClient } from "../api";

// not routable: it edits the Store blade's store, which arrives only through options
defineBlade({ url: "/store-fulfillment-centers", name: "InventoryStoreFulfillmentCenters", permissions: ["inventory:fulfillment:read"], routable: false });

type StoreCenters = {
  name?: string;
  mainFulfillmentCenterId?: string;
  additionalFulfillmentCenterIds?: string[];
  mainReturnsFulfillmentCenterId?: string;
  returnsFulfillmentCenterIds?: string[];
};
const FIELDS = ["mainFulfillmentCenterId", "additionalFulfillmentCenterIds", "mainReturnsFulfillmentCenterId", "returnsFulfillmentCenterIds"] as const;

// options.store: the Store blade's (reactive) store object
const { t } = useI18n({ useScope: "global" });
const { options, closeSelf } = useBlade<{ store: StoreCenters }>();
const client = useInventoryClient();

const store = computed(() => options.value?.store);
const edited = ref<StoreCenters>(JSON.parse(JSON.stringify(Object.fromEntries(FIELDS.map((f) => [f, store.value?.[f]])))));

async function searchCenters(keyword?: string, skip = 0, ids?: string[]) {
  const r = await (await client()).searchFulfillmentCenters({ searchPhrase: keyword, objectIds: ids, skip, take: ids?.length || 20 });
  return { results: r.results ?? [], totalCount: r.totalCount ?? 0 };
}

const bladeToolbar = ref<IBladeToolbar[]>([
  {
    id: "apply",
    title: t("INVENTORY.TOOLBAR.APPLY"),
    icon: "lucide-check",
    disabled: computed(() => !store.value),
    async clickHandler() {
      Object.assign(store.value!, edited.value);
      await closeSelf();
    },
  },
]);
</script>
