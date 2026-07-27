import 'package:json_annotation/json_annotation.dart';

part 'frizer.g.dart';

@JsonSerializable()
class Frizer {
  final int? id;
  final int? userId;
  final String? imePrezime;
  final String? email;
  final String? profileImageBase64;
  final String? biografija;
  final String? specijalizacija;
  final bool? isActive;
  final DateTime? createdAt;
  final List<int>? uslugaIds;

  Frizer({
    this.id,
    this.userId,
    this.imePrezime,
    this.email,
    this.profileImageBase64,
    this.biografija,
    this.specijalizacija,
    this.isActive,
    this.createdAt,
    this.uslugaIds,
  });

  factory Frizer.fromJson(Map<String, dynamic> json) =>
      _$FrizerFromJson(json);

  Map<String, dynamic> toJson() => _$FrizerToJson(this);
}
