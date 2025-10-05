using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Core;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// Validation service with FluentValidation integration and rule discovery
/// </summary>
public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private readonly Dictionary<Type, object> _validators;

    public ValidationService(ILogger<ValidationService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _validators = new Dictionary<Type, object>();

        _logger.LogInformation("ValidationService initialized");
    }

    /// <summary>
    /// Validate an object
    /// </summary>
    public async Task<ValidationResult> ValidateAsync<T>(T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        _logger.LogDebug("Validating object of type: {Type}", typeof(T).Name);

        var validatorType = typeof(IValidator<T>);
        if (!_validators.TryGetValue(typeof(T), out var validatorObj))
        {
            _logger.LogDebug("No validator registered for {Type}, attempting auto-discovery", typeof(T).Name);

            // Try to auto-discover validator
            var discoveredValidator = DiscoverValidator<T>();
            if (discoveredValidator != null)
            {
                _logger.LogInformation("Auto-discovered validator for {Type}", typeof(T).Name);
                _validators[typeof(T)] = discoveredValidator;
                validatorObj = discoveredValidator;
            }
            else
            {
                _logger.LogDebug("No validator found for {Type} - returning valid result", typeof(T).Name);
                // No validator found - return valid result
                return new ValidationResult
                {
                    IsValid = true,
                    Errors = new List<ValidationError>()
                };
            }
        }

        if (validatorObj is IValidator<T> validator)
        {
            var context = new ValidationContext<T>(obj);
            var result = await validator.ValidateAsync(context);

            _logger.LogInformation("Validation {Result} for {Type}. Errors: {ErrorCount}",
                result.IsValid ? "passed" : "failed", typeof(T).Name, result.Errors.Count);

            if (!result.IsValid)
            {
                _logger.LogDebug("Validation errors: {Errors}",
                    string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            }

            return new ValidationResult
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => new ValidationError
                {
                    PropertyName = e.PropertyName,
                    ErrorMessage = e.ErrorMessage,
                    Severity = e.Severity.ToString()
                }).ToList()
            };
        }

        throw new InvalidOperationException($"Validator for type {typeof(T).Name} is not of correct type");
    }

    /// <summary>
    /// Register a validator for a type
    /// </summary>
    public void RegisterValidator<T>(object validator) where T : class
    {
        ArgumentNullException.ThrowIfNull(validator);

        if (validator is not IValidator<T>)
        {
            _logger.LogError("Attempted to register invalid validator for {Type}", typeof(T).Name);
            throw new ArgumentException($"Validator must implement IValidator<{typeof(T).Name}>");
        }

        _validators[typeof(T)] = validator;
        _logger.LogInformation("Validator registered for type: {Type}", typeof(T).Name);
    }

    /// <summary>
    /// Get validation rule metadata
    /// </summary>
    public List<ValidationRuleMetadata> GetRuleMetadata<T>() where T : class
    {
        var metadata = new List<ValidationRuleMetadata>();

        if (_validators.TryGetValue(typeof(T), out var validatorObj))
        {
            if (validatorObj is IValidator<T> validator)
            {
                var descriptor = validator.CreateDescriptor();

                foreach (var member in descriptor.GetMembersWithValidators())
                {
                    foreach (var rule in descriptor.GetRulesForMember(member.Key))
                    {
                        metadata.Add(new ValidationRuleMetadata
                        {
                            RuleName = rule.GetType().Name,
                            PropertyName = member.Key,
                            Severity = "Error",
                            ErrorMessage = $"Validation rule for {member.Key}",
                            ValidatorType = validator.GetType().Name
                        });
                    }
                }
            }
        }

        return metadata;
    }

    /// <summary>
    /// Discover validator by convention (attributes, naming, assembly scanning)
    /// </summary>
    private IValidator<T>? DiscoverValidator<T>() where T : class
    {
        // Convention: Look for class named {TypeName}Validator
        var validatorTypeName = $"{typeof(T).Name}Validator";
        _logger.LogDebug("Searching for validator: {ValidatorTypeName}", validatorTypeName);

        var validatorType = typeof(T).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name == validatorTypeName && typeof(IValidator<T>).IsAssignableFrom(t));

        if (validatorType != null)
        {
            _logger.LogDebug("Found validator type: {ValidatorType}", validatorType.FullName);
            try
            {
                return Activator.CreateInstance(validatorType) as IValidator<T>;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to instantiate validator {ValidatorType}", validatorType.FullName);
                // Validator constructor might require dependencies
                return null;
            }
        }

        _logger.LogDebug("No validator found for {Type}", typeof(T).Name);
        return null;
    }
}
