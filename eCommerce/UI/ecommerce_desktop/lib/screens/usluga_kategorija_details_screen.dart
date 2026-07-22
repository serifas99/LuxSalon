import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/usluga_kategorija.dart';
import 'package:ecommerce_desktop/providers/usluga_kategorija_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:provider/provider.dart';

class UslugaKategorijaDetailsScreen extends StatefulWidget {
  final UslugaKategorija? kategorija;

  const UslugaKategorijaDetailsScreen({super.key, this.kategorija});

  @override
  State<UslugaKategorijaDetailsScreen> createState() =>
      _UslugaKategorijaDetailsScreenState();
}

class _UslugaKategorijaDetailsScreenState
    extends State<UslugaKategorijaDetailsScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  Map<String, dynamic> _initialValue = {};

  late UslugaKategorijaProvider _provider;

  @override
  void initState() {
    super.initState();

    _initialValue = {
      'naziv': widget.kategorija?.naziv ?? '',
      'opis': widget.kategorija?.opis ?? '',
      'isActive': widget.kategorija?.isActive ?? true,
    };

    _provider = context.read<UslugaKategorijaProvider>();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return MasterScreen(
      title: widget.kategorija != null
          ? 'Uredi kategoriju'
          : 'Nova kategorija',
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(32.0),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 700),
            child: Column(
              children: [
                _buildHeader(theme),
                const SizedBox(height: 24.0),
                Card(
                  elevation: 10,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12.0),
                    side: BorderSide(
                      color: theme.colorScheme.primaryContainer,
                      width: 2,
                    ),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: _buildForm(theme),
                  ),
                ),
                SizedBox(height: 24.0),
                _buildActions(theme),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeader(ThemeData theme) {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.all(10.0),
          decoration: BoxDecoration(
            color: theme.colorScheme.primaryContainer,
            borderRadius: BorderRadius.circular(8.0),
          ),
          child: Icon(
            Icons.category_outlined,
            color: theme.colorScheme.onPrimaryContainer,
          ),
        ),
        const SizedBox(width: 16.0),
        Column(
          children: [
            Text(
              widget.kategorija != null
                  ? widget.kategorija!.naziv!
                  : 'Nova kategorija',
              style: theme.textTheme.headlineSmall,
            ),
            Text(
              widget.kategorija != null
                  ? 'Uredite podatke kategorije'
                  : 'Popunite formu za novu kategoriju',
              style: theme.textTheme.bodyMedium,
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildForm(ThemeData theme) {
    return FormBuilder(
      key: _formKey,
      initialValue: _initialValue,
      child: Column(
        children: [
          FormBuilderTextField(
            name: 'naziv',
            decoration: const InputDecoration(label: Text("Naziv")),
          ),
          const SizedBox(height: 16.0),
          FormBuilderTextField(
            name: 'opis',
            decoration: const InputDecoration(label: Text("Opis")),
            maxLines: 3,
          ),
          const SizedBox(height: 16.0),
          FormBuilderCheckbox(
            name: 'isActive',
            title: const Text("Aktivna"),
          ),
        ],
      ),
    );
  }

  Widget _buildActions(ThemeData theme) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.end,
      children: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text("Otkaži"),
        ),
        const SizedBox(width: 16.0),
        ElevatedButton(
          onPressed: _save,
          child: const Text("Sačuvaj"),
        ),
      ],
    );
  }

  Future _save() async {
    if (_formKey.currentState?.saveAndValidate() ?? false) {
      var formData = _formKey.currentState!.value;

      try {
        if (widget.kategorija != null) {
          await _provider.update(widget.kategorija!.id!, formData);
        } else {
          await _provider.insert(formData);
        }

        if (!mounted) return;
        Navigator.of(context).pop("reload");
      } catch (e) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Greška prilikom čuvanja: $e")),
        );
      }
    }
  }
}
