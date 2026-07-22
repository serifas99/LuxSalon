import 'package:json_annotation/json_annotation.dart';

part 'usluga_kategorija.g.dart';

@JsonSerializable()
class UslugaKategorija {
  final int? id;
  final String? naziv;
  final String? opis;
  final bool? isActive;
  final DateTime? createdAt;

  UslugaKategorija({
    this.id,
    this.naziv,
    this.opis,
    this.isActive,
    this.createdAt,
  });

  factory UslugaKategorija.fromJson(Map<String, dynamic> json) =>
      _$UslugaKategorijaFromJson(json);

  Map<String, dynamic> toJson() => _$UslugaKategorijaToJson(this);
}
