﻿using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WeatherMapMVVM.Helpers;
using WeatherMapMVVM.Models;
using WeatherMapMVVM.Services;
using WeatherMapMVVM.ItemViewModels;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace WeatherMapMVVM.ViewModels
{
    public class ForecastPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private CityForecastResponse _forecast;
        private List<CityForecast> _listForecast = new List<CityForecast>(); //Presistencia
        private ObservableCollection<CityItemViewModel> _obseForecast;

        private bool _isRunning;
        private string _search;
        private DelegateCommand _searchCommand;

        public ForecastPageViewModel(
            INavigationService navigationService,
            IApiService apiService
            ) : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            Title = Languages.WeatherForecast;
            LoadForecastAsync();
        }

        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(ShowCities));

        public ObservableCollection<CityItemViewModel> ObseForecast
        {
            get => _obseForecast;
            set => SetProperty(ref _obseForecast, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public string Search
        {
            get => _search;
            set
            {
                SetProperty(ref _search, value);
                ShowCities();
            }
        }

        public CityForecastResponse Forecast
        {
            get => _forecast;
            set => SetProperty(ref _forecast, value);
        }

        public List<CityForecast> ListForecast
        {
            get => _listForecast;
            set => SetProperty(ref _listForecast, value);
        }

        private async void LoadForecastAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await App.Current.MainPage.DisplayAlert(
                        Languages.Error,
                        Languages.ConnectionError,
                        Languages.Accept);
                });
                return;
            }

            IsRunning = true;

            string url = App.Current.Resources["UrlAPI"].ToString();

            Response response = await _apiService.GetListAsync<CityForecastResponse>(url, "/open-data/forecast/meteorology/cities/daily", "/hp-daily-forecast-day0");

            IsRunning = false;

            if (!response.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    response.Message,
                    Languages.Accept);
                return;
            }

            Forecast = (CityForecastResponse)response.Result;

            foreach (var city in _forecast.Data)
            {
                // Converter
                _listForecast.Add(new CityForecast
                {
                    Owner = _forecast.Owner,
                    Country = _forecast.Country,
                    ForecastDate = _forecast.ForecastDate,
                    DataUpdate = _forecast.DataUpdate,

                    PrecipitaProb = city.PrecipitaProb,
                    TMin = city.TMin,
                    TMax = city.TMax,
                    PredWindDir = city.PredWindDir,
                    IdWeatherType = city.IdWeatherType,
                    ClassWindSpeed = city.ClassWindSpeed,
                    Longitude = city.Longitude,
                    ClassPrecInt = city.ClassPrecInt,
                    GlobalIdLocal = city.GlobalIdLocal,
                    Latitude = city.Latitude,
                });

                ShowCities();
            }


        }

        private void ShowCities()
        {
            if (string.IsNullOrEmpty(Search))
            {
                // Converter
                ObseForecast = new ObservableCollection<CityItemViewModel>(_listForecast.Select(p =>
                new CityItemViewModel(_navigationService)
                {
                    Owner = p.Owner,
                    Country = p.Country,
                    ForecastDate = p.ForecastDate,
                    DataUpdate = p.DataUpdate,
                    PrecipitaProb = p.PrecipitaProb,
                    TMin = p.TMin,
                    TMax = p.TMax,
                    PredWindDir = p.PredWindDir,
                    IdWeatherType = p.IdWeatherType,
                    ClassWindSpeed = p.ClassWindSpeed,
                    Longitude = p.Longitude,
                    ClassPrecInt = p.ClassPrecInt,
                    GlobalIdLocal = p.GlobalIdLocal,
                    Latitude = p.Latitude,
                }).ToList());
            }
            else
            {
                // Converter
                ObseForecast = new ObservableCollection<CityItemViewModel>(
                    _listForecast.Select(p =>
                    new CityItemViewModel(_navigationService)
                    {
                        Owner = p.Owner,
                        Country = p.Country,
                        ForecastDate = p.ForecastDate,
                        DataUpdate = p.DataUpdate,
                        PrecipitaProb = p.PrecipitaProb,
                        TMin = p.TMin,
                        TMax = p.TMax,
                        PredWindDir = p.PredWindDir,
                        IdWeatherType = p.IdWeatherType,
                        ClassWindSpeed = p.ClassWindSpeed,
                        Longitude = p.Longitude,
                        ClassPrecInt = p.ClassPrecInt,
                        GlobalIdLocal = p.GlobalIdLocal,
                        Latitude = p.Latitude,
                    }
                    ).Where(p => p.GlobalIdLocal.ToString().Contains(Search.ToString())));
            }
        }
    }
}