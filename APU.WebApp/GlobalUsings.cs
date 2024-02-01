global using System;
global using System.Text.Json;

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Components;

global using System.Diagnostics;


//global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.JSInterop;

global using EventAggregator.Blazor;

// WebApp
global using APU.WebApp.Services.SignalRHub.HubClients.ApuHub;
global using APU.WebApp.Services.SignalRHub.HubClients.ApuHub.Messages;
global using APU.WebApp.Services.SignalRHub.HubClients.BaseItemHub;
global using APU.WebApp.Services.SignalRHub.HubClients.BaseItemHub.Messages;
global using APU.WebApp.Services.SignalRHub.HubClients.ProjectHub;
global using APU.WebApp.Services.SignalRHub.HubClients.ProjectHub.Messages;
global using APU.WebApp.Shared.AppHeader;
global using APU.WebApp.Utils.Dev;
global using APU.WebApp.Utils.EventAggregatorMessages;
global using APU.WebApp.Utils.Extensions;

// Data
global using APU.DataV2.Definitions;
global using APU.DataV2.Entities;
global using APU.DataV2.EntityHelper;
global using APU.DataV2.Utils.Extensions;
global using APU.DataV2.Utils.Helpers;
global using APU.DataV2.Utils;

global using RD = APU.DataV2.Definitions.RoleDefinitions;

// Syncfusion
global using Syncfusion.Blazor.Grids;
global using SfGridAction = Syncfusion.Blazor.Grids.Action;
global using SfSplitButtonSelectionMode = Syncfusion.Blazor.SplitButtons.SelectionMode;
global using SfGanttAction = Syncfusion.Blazor.Gantt.Action;
global using SfNavigationClickEventArgs = Syncfusion.Blazor.Navigations.ClickEventArgs;
global using SfDropDownFilteringEventArgs = Syncfusion.Blazor.DropDowns.FilteringEventArgs;

global using Tegu.Blazor.Controls;