﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.InventoryModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.InventoryModule.Data.MySql.Migrations
{
    [DbContext(typeof(InventoryDbContext))]
    partial class InventoryDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("VirtoCommerce.InventoryModule.Data.Model.FulfillmentCenterDynamicPropertyObjectValueEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<bool?>("BooleanValue")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DateTimeValue")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal?>("DecimalValue")
                        .HasColumnType("decimal(18,5)");

                    b.Property<string>("DictionaryItemId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int?>("IntegerValue")
                        .HasColumnType("int");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("LongTextValue")
                        .HasColumnType("longtext");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ObjectId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ObjectType")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PropertyId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("PropertyName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("ShortTextValue")
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<string>("ValueType")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("ObjectType", "ObjectId")
                        .HasDatabaseName("IX_FulfillmentCenterDynamicPropertyObjectValue_ObjectType_ObjectId");

                    b.ToTable("FulfillmentCenterDynamicPropertyObjectValue", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.InventoryModule.Data.Model.FulfillmentCenterEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("City")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CountryCode")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("CountryName")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("DaytimePhoneNumber")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("GeoLocation")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Line1")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)");

                    b.Property<string>("Line2")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("OrganizationId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("PostalCode")
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.Property<string>("RegionId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("RegionName")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ShortDescription")
                        .HasColumnType("longtext");

                    b.Property<string>("StateProvince")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.ToTable("FulfillmentCenter", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.InventoryModule.Data.Model.InventoryEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<bool>("AllowBackorder")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("AllowPreorder")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("BackorderAvailabilityDate")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("BackorderQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("FulfillmentCenterId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.Property<decimal>("InStockQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<DateTime?>("PreorderAvailabilityDate")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("PreorderQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ReorderMinQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ReservedQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Sku")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FulfillmentCenterId");

                    b.HasIndex("Sku", "ModifiedDate")
                        .HasDatabaseName("IX_Inventory_Sku_ModifiedDate");

                    b.ToTable("Inventory", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.InventoryModule.Data.Model.FulfillmentCenterDynamicPropertyObjectValueEntity", b =>
                {
                    b.HasOne("VirtoCommerce.InventoryModule.Data.Model.FulfillmentCenterEntity", "FulfillmentCenter")
                        .WithMany("DynamicPropertyObjectValues")
                        .HasForeignKey("ObjectId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("FulfillmentCenter");
                });

            modelBuilder.Entity("VirtoCommerce.InventoryModule.Data.Model.InventoryEntity", b =>
                {
                    b.HasOne("VirtoCommerce.InventoryModule.Data.Model.FulfillmentCenterEntity", "FulfillmentCenter")
                        .WithMany()
                        .HasForeignKey("FulfillmentCenterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FulfillmentCenter");
                });

            modelBuilder.Entity("VirtoCommerce.InventoryModule.Data.Model.FulfillmentCenterEntity", b =>
                {
                    b.Navigation("DynamicPropertyObjectValues");
                });
#pragma warning restore 612, 618
        }
    }
}
