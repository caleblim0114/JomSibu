﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace JomSibu.Shared.Models;

public partial class LocationPreferencesTable
{
    public int Id { get; set; }

    public int? LocationId { get; set; }

    public int? PreferenceId { get; set; }

    public virtual LocationsTable Location { get; set; }

    public virtual PreferencesTable Preference { get; set; }
}