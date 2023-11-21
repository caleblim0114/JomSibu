﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace JomSibu.Shared.Models;

public partial class LocationsTable
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }

    public DateTime? OperationDateTime { get; set; }

    public DateTime? RecommendedDateTime { get; set; }

    public decimal? AverageReview { get; set; }

    public virtual ICollection<LocationImagesTable> LocationImagesTables { get; set; } = new List<LocationImagesTable>();

    public virtual ICollection<LocationPreferencesTable> LocationPreferencesTables { get; set; } = new List<LocationPreferencesTable>();

    public virtual ICollection<LocationReviewsTable> LocationReviewsTables { get; set; } = new List<LocationReviewsTable>();

    public virtual ICollection<UserRoutesTable> UserRoutesTables { get; set; } = new List<UserRoutesTable>();
}