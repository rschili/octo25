namespace HammingTests;

public class Tests
{

    [Test]
    public async Task RpnEvaluator_Evaluate_ResolvesTestDataset()
    {
        await Assert.That(RpnEvaluator.Evaluate("-59640 -80328 -80388 - / -61773 58754 + - ")).IsEqualTo(2025);
    }

    // Corner Cases
    [Test]
    public async Task RpnEvaluator_Evaluate_SingleNumber()
    {
        await Assert.That(RpnEvaluator.Evaluate("42")).IsEqualTo(42);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_NegativeNumber()
    {
        await Assert.That(RpnEvaluator.Evaluate("-100")).IsEqualTo(-100);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_Zero()
    {
        await Assert.That(RpnEvaluator.Evaluate("0")).IsEqualTo(0);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_SimpleAddition()
    {
        await Assert.That(RpnEvaluator.Evaluate("5 3 +")).IsEqualTo(8);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_SimpleSubtraction()
    {
        await Assert.That(RpnEvaluator.Evaluate("10 3 -")).IsEqualTo(7);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_SimpleMultiplication()
    {
        await Assert.That(RpnEvaluator.Evaluate("4 7 *")).IsEqualTo(28);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_SimpleDivision()
    {
        await Assert.That(RpnEvaluator.Evaluate("20 4 /")).IsEqualTo(5);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_NegativeResult()
    {
        await Assert.That(RpnEvaluator.Evaluate("5 10 -")).IsEqualTo(-5);
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_ExtraWhitespace()
    {
        await Assert.That(RpnEvaluator.Evaluate("  5   3   +  ")).IsEqualTo(8);
    }

    // Error Cases
    [Test]
    public async Task RpnEvaluator_Evaluate_EmptyString_ThrowsInvalidOperationException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("")).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_WhitespaceOnly_ThrowsInvalidOperationException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("   ")).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_InsufficientOperands_ThrowsInvalidOperationException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("5 +")).Throws<InvalidOperationException>().WithMessageContaining("insufficient operands");
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_TooManyOperands_ThrowsInvalidOperationException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("5 3 7")).Throws<InvalidOperationException>().WithMessageContaining("too many operands");
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_UnknownOperator_ThrowsArgumentException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("5 3 %")).Throws<ArgumentException>().WithMessageContaining("Unknown operator");
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_DivisionByZero_ThrowsDivideByZeroException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("5 0 /")).Throws<DivideByZeroException>();
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_OperatorFirst_ThrowsInvalidOperationException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("+ 5 3")).Throws<InvalidOperationException>().WithMessageContaining("insufficient operands");
    }

    [Test]
    public async Task RpnEvaluator_Evaluate_InvalidToken_ThrowsArgumentException()
    {
        await Assert.That(() => RpnEvaluator.Evaluate("5 3 abc")).Throws<ArgumentException>().WithMessageContaining("Unknown operator");
    }
}
