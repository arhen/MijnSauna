﻿using System.Threading.Tasks;
using Grpc.Core;
using MijnSauna.Common.Protobuf;
using MijnSauna.Middleware.Processor.Services.Interfaces;

namespace MijnSauna.Middleware.Processor.Services
{
    public class GrpcService : SaunaService.SaunaServiceBase
    {
        private readonly IGpioService _gpioService;

        public GrpcService(
            IGpioService gpioService)
        {
            _gpioService = gpioService;
        }

        public override async Task<GetTemperatureResponse> GetTemperature(GetTemperatureRequest request, ServerCallContext context)
        {
            _gpioService.Initialize();
            var temperature = await _gpioService.ReadTemperature();

            return new GetTemperatureResponse
            {
                Temperature = temperature
            };
        }

        public override Task<GetStateResponse> GetState(GetStateRequest request, ServerCallContext context)
        {
            _gpioService.Initialize();
            var isSaunaOn  = _gpioService.IsSaunaOn();
            var isInfraredOn = _gpioService.IsSaunaOn();

            return Task.FromResult(new GetStateResponse
            {
                IsSaunaOn = isSaunaOn,
                IsInfraredOn = isInfraredOn
            });
        }
    }
}