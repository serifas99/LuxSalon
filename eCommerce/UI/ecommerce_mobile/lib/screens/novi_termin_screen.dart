import 'package:ecommerce_mobile/models/frizer.dart';
import 'package:ecommerce_mobile/models/usluga.dart';
import 'package:ecommerce_mobile/providers/auth_provider.dart';
import 'package:ecommerce_mobile/providers/termin_provider.dart';
import 'package:ecommerce_mobile/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class NoviTerminScreen extends StatefulWidget {
  final Usluga usluga;
  final List<Frizer> frizeri;
  final Frizer? odabraniFrizer;

  const NoviTerminScreen({
    super.key,
    required this.usluga,
    required this.frizeri,
    this.odabraniFrizer,
  });

  @override
  State<NoviTerminScreen> createState() => _NoviTerminScreenState();
}

class _NoviTerminScreenState extends State<NoviTerminScreen> {
  late TerminProvider _terminProvider;

  Frizer? _frizer;
  DateTime? _datum;
  TimeOfDay? _vrijeme;
  final TextEditingController _napomenaController = TextEditingController();

  bool _saving = false;

  int get _klijentId =>
      int.tryParse(AuthProvider.accessTokenDecoded?['Id']?.toString() ?? '') ??
      0;

  @override
  void initState() {
    super.initState();
    _terminProvider = context.read<TerminProvider>();
    _frizer = widget.odabraniFrizer;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Novi termin")),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              widget.usluga.naziv ?? '',
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            Text(
              "${widget.usluga.cijena ?? 0} KM • ${widget.usluga.trajanjeMinuta ?? 0} min",
              style: TextStyle(color: Colors.grey.shade600),
            ),
            const SizedBox(height: 24),
            const Text("Frizer", style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            DropdownButtonFormField<Frizer>(
              initialValue: _frizer,
              decoration: const InputDecoration(border: OutlineInputBorder()),
              hint: const Text("Izaberi frizera"),
              items: widget.frizeri
                  .map((f) => DropdownMenuItem(
                        value: f,
                        child: Text(f.imePrezime ?? ''),
                      ))
                  .toList(),
              onChanged: (value) => setState(() => _frizer = value),
            ),
            const SizedBox(height: 20),
            const Text("Datum", style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            InkWell(
              onTap: () async {
                final picked = await showDatePicker(
                  context: context,
                  initialDate: DateTime.now().add(const Duration(days: 1)),
                  firstDate: DateTime.now(),
                  lastDate: DateTime.now().add(const Duration(days: 180)),
                );
                if (picked != null) setState(() => _datum = picked);
              },
              child: InputDecorator(
                decoration: const InputDecoration(border: OutlineInputBorder()),
                child: Text(_datum != null
                    ? "${_datum!.day.toString().padLeft(2, '0')}.${_datum!.month.toString().padLeft(2, '0')}.${_datum!.year}."
                    : "Izaberi datum"),
              ),
            ),
            const SizedBox(height: 20),
            const Text("Vrijeme", style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            InkWell(
              onTap: () async {
                final picked = await showTimePicker(
                  context: context,
                  initialTime: const TimeOfDay(hour: 10, minute: 0),
                );
                if (picked != null) setState(() => _vrijeme = picked);
              },
              child: InputDecorator(
                decoration: const InputDecoration(border: OutlineInputBorder()),
                child: Text(_vrijeme != null
                    ? _vrijeme!.format(context)
                    : "Izaberi vrijeme"),
              ),
            ),
            const SizedBox(height: 20),
            const Text("Napomena (opciono)",
                style: TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            TextField(
              controller: _napomenaController,
              maxLines: 3,
              decoration: const InputDecoration(border: OutlineInputBorder()),
            ),
            const SizedBox(height: 28),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _saving ? null : _zakazi,
                child: _saving
                    ? const SizedBox(
                        height: 18,
                        width: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text("Zakaži termin"),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future _zakazi() async {
    if (_frizer == null) {
      alertBox(context, "Greška", "Izaberi frizera.");
      return;
    }
    if (_datum == null || _vrijeme == null) {
      alertBox(context, "Greška", "Izaberi datum i vrijeme.");
      return;
    }

    final datumVrijeme = DateTime(
      _datum!.year,
      _datum!.month,
      _datum!.day,
      _vrijeme!.hour,
      _vrijeme!.minute,
    );

    setState(() => _saving = true);

    try {
      await _terminProvider.insert({
        "klijentId": _klijentId,
        "frizerId": _frizer!.id,
        "uslugaId": widget.usluga.id,
        "datumVrijeme": datumVrijeme.toIso8601String(),
        "napomena": _napomenaController.text,
      });

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Termin je uspješno zakazan!")),
      );
      Navigator.pop(context, "reload");
    } on Exception catch (e) {
      setState(() => _saving = false);
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }
}
