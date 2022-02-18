﻿using Plugin.Geolocator;
using Plugin.Media;
using PM02E10249.Controllers;
using PM02E10249.Models;
using PM02E10249.Views;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Threading;

namespace PM02E10249.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        byte[] imgtoSave;

        public MainPage()
        {
            InitializeComponent();
            LongitudLatitud();
        }

        public async void LongitudLatitud()
        {
            try
            {
                var georequest = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));

                var tokendecancelacion = new CancellationTokenSource();

                var localizacion = await Geolocation.GetLocationAsync(georequest, tokendecancelacion.Token);


                if (localizacion != null)
                {
                    Latitud_input.Text = localizacion.Latitude.ToString();
                    Longitud_input.Text = localizacion.Longitude.ToString();
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("PM2E10249", "Este dispositivo no soporta GPS", "Continuar");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("PM2E10249", "Error de Dispositivo", "Continuar");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("PM2E10249", "Sin Permisos de Geolocalizacion", "Ya me voy pues");
            }
            catch (Exception ex)
            {
                await DisplayAlert("PM2E10249", "Sin Ubicacion", "Continuar");
            }
        }

        private async void btntomarphoto_Clicked(object sender, EventArgs e)
        {
            var tomarphoto = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "PhotoLocationApp",
                Name = DateTime.Now.ToString() + "_Photo.jpg",
                SaveToAlbum = true
            });

            if (tomarphoto != null)
            {
                imgtoSave = null;
                MemoryStream memoryStream = new MemoryStream();

                tomarphoto.GetStream().CopyTo(memoryStream);
                imgtoSave = memoryStream.ToArray();

                imgphotoubicacion.Source = ImageSource.FromStream(() => { return tomarphoto.GetStream(); });
            }
        }

        private byte[] GetImgBytes(Stream stream)
        {
            byte[] ImgBytes;
            using (var memoryStream = new System.IO.MemoryStream())
            {
                stream.CopyTo(memoryStream);
                ImgBytes = memoryStream.ToArray();
            }
            return ImgBytes;
        }

        private async void btnaguardar_Clicked(object sender, EventArgs e)
        {
            var ubicacion = new Ubicaciones
            {
                Imagen = imgtoSave,
                Longitud = (float)Convert.ToDouble(Latitud_input.Text),
                Latitud = (float)Convert.ToDouble(Latitud_input.Text),
                Descripcion = Descripcion_input.Text
            };

            var resultado = await App.BaseDatos.GuardarUbicacion(ubicacion);

            if (resultado != 0)
            {
                await DisplayAlert("Aviso", "Ubicacion Guardada con Exito!!!", "Ok");
                await Navigation.PushAsync(new ListViewLocationsPage());
            }
            else
            {
                await DisplayAlert("Aviso", "Ha Ocurrido un Error", "Ok");
            }
        }

        private async void btnlistviewubicaciones_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ListViewLocationsPage());
        }

        private void btnsalir_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}