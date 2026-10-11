using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Toltech.App.Front;
using Toltech.App.ViewModels;
using Toltech.App.Models;
using Toltech.App.Services;


namespace Toltech.App.FrontEnd.Controls
{
    public partial class PanelReqs : UserControl
    {
        private string currentContextTarget = string.Empty; // "Part1" ou "Part2"

        #region Constructeur
        public PanelReqs()
        {
            InitializeComponent();
            //Debug.WriteLine("PANEL CREATED -> " + Guid.NewGuid());

            
        }

        public RequirementsViewModel ParentViewModel
        {
            get => (RequirementsViewModel)GetValue(ParentViewModelProperty);
            set => SetValue(ParentViewModelProperty, value);
        }

        public static readonly DependencyProperty ParentViewModelProperty =
            DependencyProperty.Register(
                nameof(ParentViewModel),
                typeof(RequirementsViewModel),
                typeof(PanelReqs),
                new PropertyMetadata(null));


        // Création d'une DependencyProperty pour faciliter le binding
        public static readonly DependencyProperty RequirementProperty =
            DependencyProperty.Register(
                nameof(Requirement),
                typeof(Requirements),
                typeof(PanelReqs),
                 new PropertyMetadata(null));

        public Requirements Requirement
        {
            get => (Requirements)GetValue(RequirementProperty);
            set => SetValue(RequirementProperty, value);
        }

        #endregion


    }
}
