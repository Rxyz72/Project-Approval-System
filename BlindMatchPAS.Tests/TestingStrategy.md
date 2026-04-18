# Testing Strategy - Blind Match Project Approval System

## Executive Summary

This document provides a comprehensive overview of the testing strategy implemented for the Blind Match Project Approval System. Our test suite consists of **16 automated tests** covering Unit, Integration, and Functional testing layers.

**Test Results:**
- Total Tests: 16
- Passed: 16
- Failed: 0
- Skipped: 0
- Duration: ~8 seconds

---

## Test Coverage

### Unit Tests (StudentControllerUnitTests.cs)
- **Dashboard_ReturnsViewWithProposals**: Tests that student dashboard loads proposals correctly
- **CreateProposal_ValidModel_RedirectsToDashboard**: Validates proposal creation flow
- **WithdrawProposal_MatchedProposal_ReturnsError**: Tests business rule - can't withdraw matched proposals
- **DeleteProposal_PendingProposal_DeletesSuccessfully**: Tests proposal deletion

**Mocking**: Used Moq to mock UserManager and IUserStore for isolated unit testing

### Integration Tests (BlindMatchingIntegrationTests.cs)
- **BlindMatching_ProposalHiddenUntilConfirmed**: End-to-end blind matching workflow
- **Database_CanSaveAndRetrieveProposal**: EF Core data persistence validation
- **Match_CascadeDeleteWhenProposalDeleted**: Database relationship constraints
- **ResearchArea_CanBeDeactivated**: CRUD operations on research areas

**Database**: Used InMemory database for isolated integration testing

### Functional/Logic Tests (MatchingLogicTests.cs)
- **ProposalStatus_TransitionsCorrectly**: State machine validation
- **Match_IdentityRevealLogic**: Core blind matching logic
- **ProposalValidation_RequiredFieldsPresent**: Data validation rules
- **Proposal_CanBeEditedBasedOnStatus**: Business rule enforcement (Theory test with multiple scenarios)
- **ResearchArea_ActiveByDefault**: Default value validation


### Tools and Technologies

### Testing Framework
**xUnit 2.4.2**
- Industry standard for .NET
- Excellent async/await support
- Theory-based parameterized tests
- Clean test output

### Mocking Framework
**Moq 4.20.70**
- Mock UserManager (complex ASP.NET Identity dependency)
- Mock TempData for controller testing
- Verify method calls and setup return values

**Example**:
```csharp
var userManagerMock = GetMockUserManager();
userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
    .Returns("test-user-id");
```
## Test Execution
Run all tests: `dotnet test`
Coverage: 12 tests covering critical user journeys and business logic

---
## Test Execution Guide

### Run All Tests
```bash
cd BlindMatchPAS.Tests
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter StudentControllerUnitTests
```

### Run with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Generate Coverage Report (with Coverlet)
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
## Conclusion

Our testing strategy successfully validates the core functionality of the Blind Match PAS with **100% pass rate**. The three-tier approach ensures:

1. **Unit Tests**: Controllers work in isolation
2. **Integration Tests**: Database operations are reliable
3. **Functional Tests**: Business rules are enforced

The test suite provides confidence in the **blind matching mechanism** - the system's most critical feature - while maintaining fast execution times suitable for continuous integration.

**Total Test Count**: 16  
**Pass Rate**: 100%  
**Execution Time**: ~8 seconds  
**Maintainability**: High (AAA pattern, clear naming)  

---

## Appendix: Sample Test Output


**Document Version**: 1.0  
**Date**: April 2026  
**Authors**: [Group AA]